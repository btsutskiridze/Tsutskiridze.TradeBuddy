using MarketData;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Net.WebSockets;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Events;
using Tsutskiridze.TradeBuddy.Core.Enums;
using Tsutskiridze.TradeBuddy.Core.Helpers;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Context;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Services.StockPrice
{
    public sealed class StockPriceWebSocketListener : BackgroundService, INotificationHandler<StockWatchStatusChanged>
    {
        private const string WssUrl = "wss://streamer.finance.yahoo.com/?version=2";
        private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(10);
        private const int WebSocketBufferSize = 8192;
        private const int AlertRateLimitSeconds = 10;
        private const int AlertMaxCountBeforeRemoval = 5;

        private readonly ILogger<StockPriceWebSocketListener> _log;
        private readonly IServiceScopeFactory _scopes;
        private readonly ITelegramBotClient _bot;
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        private ImmutableHashSet<string> _watched = ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase);
        private ClientWebSocket? _ws;

        public StockPriceWebSocketListener(
            IServiceScopeFactory scopes,
            ILogger<StockPriceWebSocketListener> log,
            ITelegramBotClient bot)
        {
            _scopes = scopes;
            _log = log;
            _bot = bot;
        }

        protected override async Task ExecuteAsync(CancellationToken stopping)
        {
            _log.LogInformation("WS listener starting (Mediator edition)");

            // Initial DB sync
            _watched = await LoadWatchedSymbolsAsync(stopping);

            while (!stopping.IsCancellationRequested)
            {
                using var ws = new ClientWebSocket();
                _ws = ws;
                try
                {
                    await ws.ConnectAsync(new Uri(WssUrl), stopping);
                    _log.LogInformation("Connected to {Url}", WssUrl);

                    if (_watched.Any())
                        await SendSubscriptionAsync(_watched, Array.Empty<string>(), stopping);

                    await ProcessMessagesAsync(stopping); // returns when socket closes / token cancelled
                }
                catch (OperationCanceledException) when (stopping.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Listener fault – will reconnect in {Delay}s", ReconnectDelay.TotalSeconds);
                }
                finally { _ws = null; }

                if (!stopping.IsCancellationRequested)
                    await Task.Delay(ReconnectDelay, stopping);
            }
            _log.LogInformation("WS listener stopped.");
        }

        public async ValueTask Handle(
            StockWatchStatusChanged notification,
            CancellationToken ct)
        {
            if (_ws?.State != WebSocketState.Open)
                return;

            var shouldSubscribe = notification.IsWatched && !_watched.Contains(notification.Symbol);
            var shouldUnsubscribe = !notification.IsWatched && _watched.Contains(notification.Symbol);

            var subscribe = shouldSubscribe ? [notification.Symbol] : Array.Empty<string>();
            var unsubscribe = shouldUnsubscribe ? [notification.Symbol] : Array.Empty<string>();

            ImmutableInterlocked.Update(ref _watched, current =>
                notification.IsWatched
                    ? current.Add(notification.Symbol)
                    : current.Remove(notification.Symbol)
            );

            if (subscribe.Length == 0 && unsubscribe.Length == 0)
                return;

            try
            {
                await _sendLock.WaitAsync(ct);

                if (ct.IsCancellationRequested)
                    return;

                await SendSubscriptionAsync(subscribe, unsubscribe, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _log.LogInformation("Handle cancelled for symbol {Symbol}", notification.Symbol);
            }
            catch (Exception ex)
            {
                _log.LogError(
                    ex,
                    "Error sending subscribe/unsubscribe for {Symbol}",
                    notification.Symbol
                );
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private async Task<ImmutableHashSet<string>> LoadWatchedSymbolsAsync(CancellationToken ct)
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var list = await db.Stocks.AsNoTracking().Where(s => s.IsWatched)
                              .Select(s => s.Symbol).ToListAsync(ct);
            return list.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private async Task SendSubscriptionAsync(IEnumerable<string> subscribe, IEnumerable<string> unsubscribe, CancellationToken ct)
        {
            if (_ws?.State != WebSocketState.Open) return;
            if (!subscribe.Any() && !unsubscribe.Any()) return;

            var payload = new JObject();
            if (subscribe.Any()) payload["subscribe"] = JArray.FromObject(subscribe);
            if (unsubscribe.Any()) payload["unsubscribe"] = JArray.FromObject(unsubscribe);

            var bytes = Encoding.UTF8.GetBytes(payload.ToString(Formatting.None));
            await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
            _log.LogInformation("WS sub update +[{Sub}] -[{Unsub}]", string.Join(',', subscribe), string.Join(',', unsubscribe));
        }

        private async Task ProcessMessagesAsync(CancellationToken ct)
        {
            var buffer = new ArraySegment<byte>(new byte[WebSocketBufferSize]);
            using var mem = new MemoryStream();

            while (!ct.IsCancellationRequested && _ws?.State == WebSocketState.Open)
            {
                mem.SetLength(0);
                WebSocketReceiveResult res;
                do
                {
                    res = await _ws.ReceiveAsync(buffer, ct);
                    if (res.MessageType == WebSocketMessageType.Close) return;
                    mem.Write(buffer.Array!, buffer.Offset, res.Count);
                } while (!res.EndOfMessage);

                var json = Encoding.UTF8.GetString(mem.ToArray());
                await HandleSingleMessageAsync(json, ct);
            }
        }

        private async Task HandleSingleMessageAsync(string json, CancellationToken ct)
        {
            try
            {
                var obj = JObject.Parse(json);
                var base64 = obj["message"]?.ToString();
                if (string.IsNullOrEmpty(base64)) return;
                var update = PricingData.Parser.ParseFrom(Convert.FromBase64String(base64));

                _log.LogInformation("WS update: {Symbol} {Price}", update.Id, update.Price);
                await HandleAlertsAsync(update, ct); // original alert body kept (omitted for brevity)
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to handle frame");
            }
        }

        private async Task HandleAlertsAsync(PricingData update, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;

            try
            {
                using var scope = _scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var threshold = DateTime.UtcNow.AddSeconds(-AlertRateLimitSeconds);

                var alerts = await db.PriceAlerts
                    .Where(pa => pa.Stock.Symbol == update.Id &&
                                 (pa.Direction == PriceAlertDirection.Above && (decimal)update.Price >= pa.Price ||
                                  pa.Direction == PriceAlertDirection.Below && (decimal)update.Price <= pa.Price) &&
                                 (pa.UpdatedAt == null || pa.UpdatedAt < threshold))
                    .Include(pa => pa.Chat)
                    .Include(pa => pa.Stock)
                    .ToListAsync(ct);

                if (!alerts.Any()) return;

                var currency = CurrencyHelper.GetCurrencySymbol(alerts[0].Stock.Currency);

                foreach (var alert in alerts)
                {
                    if (ct.IsCancellationRequested) break;
                    try
                    {
                        var dirEmoji = alert.Direction == PriceAlertDirection.Above ? "🚀" : "📉";
                        var dirText = alert.Direction == PriceAlertDirection.Above ? "Above" : "Below";

                        await _bot.SendMessage(
                            chatId: alert.Chat.TelegramChatID,
                            text: $"🔔 *{alert.Stock.Symbol}*: {currency}{update.Price:N2} 🔔" +
                                  $"{dirEmoji} Price {dirText} Target {currency}{alert.Price:N2} {dirEmoji}",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: ct);

                        alert.AlertCount++;
                        alert.UpdatedAt = DateTime.UtcNow;

                        if (alert.AlertCount >= AlertMaxCountBeforeRemoval)
                        {
                            var otherExists = await db.PriceAlerts
                                .AnyAsync(pa => pa.StockID == alert.StockID && pa.ID != alert.ID, ct);

                            if (!otherExists)
                                alert.Stock.IsWatched = false;

                            db.PriceAlerts.Remove(alert);

                            await _bot.SendMessage(
                                chatId: alert.Chat.TelegramChatID,
                                text: $"🚫 *{alert.Stock.Symbol}* Alert ({dirText} {currency}{alert.Price:N2}) removed after {AlertMaxCountBeforeRemoval} notifications. 🚫",
                                parseMode: ParseMode.Markdown,
                                cancellationToken: ct);
                        }
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "Error processing alert {AlertId}", alert.ID);
                    }
                }

                await db.SaveChangesAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unhandled error while processing alerts for {Symbol}", update.Id);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("WS listener StopAsync – shutting down");
            await base.StopAsync(cancellationToken);
        }
    }
}