using MarketData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;
using Telegram.Bot;
using Tsutskiridze.TradeBuddy.Core.Enums;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Context;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Services
{
    public class StockPriceWebSocketListener : BackgroundService
    {
        private const string WSS_URL = "wss://streamer.finance.yahoo.com/?version=2";

        private readonly ILogger<StockPriceWebSocketListener> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITelegramBotClient _botClient;

        private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(30);
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        private ClientWebSocket _ws = null!;
        private HashSet<string> _currentSymbols = [];

        public StockPriceWebSocketListener(
            IServiceScopeFactory scopeFactory,
            ILogger<StockPriceWebSocketListener> logger,
            ITelegramBotClient botClient)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _botClient = botClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri(WSS_URL), stoppingToken);
            _logger.LogInformation("WebSocket connected to {Url}", WSS_URL);

            _currentSymbols = await LoadWatchedSymbolsAsync(stoppingToken);

            if (_currentSymbols.Count != 0)
            {
                await SendSubscriptionAsync(subscribe: _currentSymbols, unsubscribe: [], stoppingToken);
            }

            var receiveTask = ProcessMessagesAsync(stoppingToken);
            var refreshTask = RefreshSubscriptionsLoopAsync(stoppingToken);

            await Task.WhenAll(receiveTask, refreshTask);

            _logger.LogInformation("WebSocket listener stopped.");
        }

        private async Task<HashSet<string>> LoadWatchedSymbolsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var symbols = await db.Stocks
                .Where(s => s.IsWatched)
                .Select(s => s.Symbol)
                .ToListAsync(ct);

            return symbols.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private async Task RefreshSubscriptionsLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _ws.State == WebSocketState.Open)
            {
                await Task.Delay(_refreshInterval, ct);

                var fresh = await LoadWatchedSymbolsAsync(ct);

                // compute diffs
                var toUnsub = _currentSymbols.Except(fresh).ToList();
                var toSub = fresh.Except(_currentSymbols).ToList();

                if (toUnsub.Count != 0 || toSub.Count != 0)
                {
                    await SendSubscriptionAsync(toSub, toUnsub, ct);

                    _currentSymbols = fresh;
                }
            }
        }

        private async Task SendSubscriptionAsync(
            IEnumerable<string> subscribe,
            IEnumerable<string> unsubscribe,
            CancellationToken ct)
        {
            var payload = new JObject();
            if (subscribe.Any())
                payload["subscribe"] = JArray.FromObject(subscribe);
            if (unsubscribe.Any())
                payload["unsubscribe"] = JArray.FromObject(unsubscribe);

            var text = payload.ToString(Formatting.None);
            var bytes = Encoding.UTF8.GetBytes(text);

            await _sendLock.WaitAsync(ct);
            try
            {
                await _ws.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: ct);

                _logger.LogInformation(
                    "Refreshed subscriptions. Subscribed to [{0}], Unsubscribed from [{1}]",
                    string.Join(", ", subscribe),
                    string.Join(", ", unsubscribe)
                );
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private async Task ProcessMessagesAsync(CancellationToken ct)
        {
            var buffer = new byte[8192];
            while (!ct.IsCancellationRequested && _ws.State == WebSocketState.Open)
            {
                var result = await _ws.ReceiveAsync(buffer, ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogWarning("Server requested close: {Status}—{Desc}",
                        result.CloseStatus, result.CloseStatusDescription);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var obj = JObject.Parse(msg);
                    var encoded = obj["message"]?.ToString();
                    if (encoded is null) continue;

                    var data = Convert.FromBase64String(encoded);
                    try
                    {
                        var update = PricingData.Parser.ParseFrom(data);

                        _logger.LogInformation("Received update: {Symbol} - {Price}", update.Id, update.Price);

                        await HandleAlertsAsync(update, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to parse protobuf message");
                    }
                }
            }
        }

        private async Task HandleAlertsAsync(PricingData update, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // eager‐load the chat so we get its TelegramChatID
            var matching = await db.PriceAlerts
                .Include(pa => pa.Chat)
                .Include(pa => pa.Stock)
                .Where(pa =>
                    pa.Stock.Symbol == update.Id
                    && (
                        (pa.Direction == PriceAlertDirection.Above && (decimal)update.Price >= pa.Price) ||
                        (pa.Direction == PriceAlertDirection.Below && (decimal)update.Price <= pa.Price)
                    )
                )
                .ToListAsync(ct);

            if (matching.Count == 0) return;

            for (int i = 0; i < matching.Count; i++)
            {
                Core.DBEntities.Telegram.PriceAlert? alert = matching[i];
                var directionText = alert.Direction == PriceAlertDirection.Above
                    ? "risen above"
                    : "fallen below";

                var msg = $"🔔 *Price Alert* 🔔\n" +
                          $"{alert.Stock.Symbol} has {directionText} your target of {alert.Stock.Currency} {alert.Price}. \n" +
                          $"Current: {alert.Stock.Currency} {update.Price}";

                await _botClient.SendMessage(
                    chatId: alert.Chat.TelegramChatID,
                    text: msg,
                    parseMode: global::Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: ct
                );

                //db.PriceAlerts.Remove(alert);
            }

            await db.SaveChangesAsync(ct);
        }
    }
}

