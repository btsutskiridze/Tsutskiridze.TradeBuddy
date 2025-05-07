using MarketData; // Assuming this is your Protobuf generated class
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
using Tsutskiridze.TradeBuddy.Core.Enums; // Assuming this exists
using Tsutskiridze.TradeBuddy.Core.Helpers; // Assuming this exists
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Context; // Assuming this exists

namespace Tsutskiridze.TradeBuddy.Infrastructure.Services
{
    public class StockPriceWebSocketListener : BackgroundService
    {
        // Configuration - In a real app, these would come from IOptions<MyConfig>
        private const string WSS_URL = "wss://streamer.finance.yahoo.com/?version=2";
        private static readonly TimeSpan REFRESH_SUBSCRIPTIONS_INTERVAL = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan RECONNECT_DELAY = TimeSpan.FromSeconds(10); // Delay before trying to reconnect
        private const int WEBSOCKET_BUFFER_SIZE = 8192;
        private const int ALERT_RATE_LIMIT_SECONDS = 3; // Cooldown for a specific alert
        private const int ALERT_MAX_COUNT_BEFORE_REMOVAL = 5; // Remove alert after this many triggers

        private readonly ILogger<StockPriceWebSocketListener> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITelegramBotClient _botClient;

        private readonly SemaphoreSlim _sendLock = new(1, 1);

        // Volatile as it's accessed by multiple tasks (ExecuteAsync, RefreshSubscriptionsLoopAsync)
        // and needs to reflect the latest state from the DB.
        private ImmutableHashSet<string> _currentSymbols = ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase);
        private ClientWebSocket? _ws;

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
            _logger.LogInformation("StockPriceWebSocketListener starting.");

            stoppingToken.Register(() => _logger.LogInformation("StockPriceWebSocketListener stopping token triggered."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _ws = new ClientWebSocket();
                try
                {
                    _logger.LogInformation("Attempting to connect to WebSocket: {Url}", WSS_URL);
                    // Configure any WebSocket options if necessary, e.g., KeepAliveInterval
                    // _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20); 
                    await _ws.ConnectAsync(new Uri(WSS_URL), stoppingToken);
                    _logger.LogInformation("WebSocket connected to {Url}", WSS_URL);

                    // Load initial symbols and subscribe
                    _currentSymbols = await LoadWatchedSymbolsAsync(stoppingToken);
                    if (_currentSymbols.Count != 0)
                    {
                        await SendSubscriptionAsync(_currentSymbols, Enumerable.Empty<string>(), stoppingToken);
                    }
                    else
                    {
                        _logger.LogInformation("No symbols currently watched. Waiting for subscriptions to change.");
                    }

                    var receiveTask = ProcessMessagesAsync(stoppingToken);
                    var refreshTask = RefreshSubscriptionsLoopAsync(stoppingToken);

                    // Wait for either task to complete (which might indicate a disconnect or an error)
                    // or for the stopping token to be cancelled.
                    await Task.WhenAny(receiveTask, refreshTask);

                    // If we reach here and not due to cancellation, it implies one of the tasks ended,
                    // possibly due to connection loss. The loop will attempt to reconnect.
                    if (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Stopping token initiated shutdown while tasks were running.");
                        break;
                    }

                    _logger.LogWarning("A processing task ended prematurely (receiveTask Status: {StatusR}, refreshTask Status: {StatusF}). WebSocket State: {State}. Attempting to reconnect.",
                        receiveTask.Status, refreshTask.Status, _ws?.State);

                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Operation cancelled. StockPriceWebSocketListener is stopping.");
                    break; // Exit loop if stoppingToken was cancelled
                }
                catch (WebSocketException wsEx)
                {
                    _logger.LogError(wsEx, "WebSocketException occurred. State: {State}. Attempting to reconnect. Error: {ErrorMessage}", _ws?.State, wsEx.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception in ExecuteAsync. Attempting to reconnect.");
                }
                finally
                {
                    if (_ws != null)
                    {
                        if (_ws.State == WebSocketState.Open || _ws.State == WebSocketState.CloseReceived)
                        {
                            _logger.LogInformation("Closing WebSocket connection.");
                            try
                            {
                                // Give a short timeout for graceful close, but don't wait indefinitely if stopping
                                var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Service shutting down or reconnecting", CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, closeCts.Token).Token);
                            }
                            catch (OperationCanceledException)
                            {
                                _logger.LogWarning("WebSocket close operation timed out or was cancelled.");
                            }
                            catch (Exception closeEx)
                            {
                                _logger.LogError(closeEx, "Exception during WebSocket CloseAsync.");
                            }
                        }
                        _ws.Dispose();
                        _ws = null;
                    }
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Waiting {Delay} seconds before attempting to reconnect.", RECONNECT_DELAY.TotalSeconds);
                    try
                    {
                        await Task.Delay(RECONNECT_DELAY, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Delay before reconnect cancelled. StockPriceWebSocketListener is stopping.");
                        break; // Exit loop if stoppingToken was cancelled during delay
                    }
                }
            }
            _logger.LogInformation("StockPriceWebSocketListener stopped.");
        }

        private async Task<ImmutableHashSet<string>> LoadWatchedSymbolsAsync(CancellationToken ct)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var symbols = await db.Stocks
                    .AsNoTracking() // Good for read-only scenarios
                    .Where(s => s.IsWatched)
                    .Select(s => s.Symbol)
                    .ToListAsync(ct);

                return symbols.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load watched symbols from database.");
                return ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase); // Return empty set on error to avoid breaking flow
            }
        }

        private async Task RefreshSubscriptionsLoopAsync(CancellationToken ct)
        {
            _logger.LogInformation("RefreshSubscriptionsLoopAsync started.");
            while (!ct.IsCancellationRequested && _ws?.State == WebSocketState.Open)
            {
                try
                {
                    await Task.Delay(REFRESH_SUBSCRIPTIONS_INTERVAL, ct);

                    if (ct.IsCancellationRequested || _ws?.State != WebSocketState.Open) break;

                    var freshSymbols = await LoadWatchedSymbolsAsync(ct);

                    // Make a local copy of _currentSymbols for thread safety during comparison
                    var currentSymbolsSnapshot = new HashSet<string>(_currentSymbols, StringComparer.OrdinalIgnoreCase);

                    var toUnsubscribe = currentSymbolsSnapshot.Except(freshSymbols).ToList();
                    var toSubscribe = freshSymbols.Except(currentSymbolsSnapshot).ToList();

                    if (toUnsubscribe.Count != 0 || toSubscribe.Count != 0)
                    {
                        await SendSubscriptionAsync(toSubscribe, toUnsubscribe, ct);
                        _currentSymbols = freshSymbols; // Update the shared set
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    _logger.LogInformation("RefreshSubscriptionsLoopAsync cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RefreshSubscriptionsLoopAsync. Loop will continue if possible.");
                    // Optionally, add a shorter delay here if errors are frequent to avoid spamming logs/retries
                    if (_ws?.State != WebSocketState.Open) break; // Exit if WS is no longer open
                    await Task.Delay(TimeSpan.FromSeconds(5), ct); // Brief pause after an error
                }
            }
            _logger.LogInformation("RefreshSubscriptionsLoopAsync stopped. WebSocket State: {State}", _ws?.State);
        }

        private async Task SendSubscriptionAsync(
            IEnumerable<string> subscribe,
            IEnumerable<string> unsubscribe,
            CancellationToken ct)
        {
            if (_ws?.State != WebSocketState.Open)
            {
                _logger.LogWarning("WebSocket is not open. Cannot send subscription.");
                return;
            }

            if (!subscribe.Any() && !unsubscribe.Any())
            {
                _logger.LogDebug("No symbols to subscribe or unsubscribe.");
                return;
            }

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
                if (_ws?.State == WebSocketState.Open) // Double check state after acquiring lock
                {
                    await _ws.SendAsync(
                        new ArraySegment<byte>(bytes),
                        WebSocketMessageType.Text,
                        endOfMessage: true,
                        cancellationToken: ct);

                    _logger.LogInformation(
                        "Sent subscription update. Subscribed: [{Subscribed}], Unsubscribed: [{Unsubscribed}]",
                        string.Join(", ", subscribe),
                        string.Join(", ", unsubscribe)
                    );
                }
                else
                {
                    _logger.LogWarning("WebSocket closed before sending subscription message after acquiring lock.");
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("Subscription send operation cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send subscription message.");
                // If send fails, the connection might be broken. The main loop should handle this.
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private async Task ProcessMessagesAsync(CancellationToken ct)
        {
            _logger.LogInformation("ProcessMessagesAsync started.");
            var buffer = new ArraySegment<byte>(new byte[WEBSOCKET_BUFFER_SIZE]);

            using var messageStream = new MemoryStream();

            while (!ct.IsCancellationRequested && _ws?.State == WebSocketState.Open)
            {
                try
                {
                    messageStream.Seek(0, SeekOrigin.Begin); // Reset stream for new message
                    messageStream.SetLength(0);
                    WebSocketReceiveResult result;
                    do
                    {
                        if (_ws == null || _ws.State != WebSocketState.Open)
                        {
                            _logger.LogWarning("WebSocket is not open or null in ProcessMessagesAsync receive loop.");
                            return; // Exit task
                        }
                        result = await _ws.ReceiveAsync(buffer, ct);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            _logger.LogWarning("Server requested WebSocket close: {Status} - {Description}",
                                result.CloseStatus, result.CloseStatusDescription);
                            // Attempt to acknowledge the close if initiated by server
                            if (_ws.State == WebSocketState.CloseReceived && result.CloseStatus.HasValue)
                            {
                                await _ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription ?? "Acknowledging server close", CancellationToken.None);
                            }
                            return; // Exit task as connection is closing/closed
                        }

                        if (buffer.Array != null) // Null check for buffer.Array
                        {
                            messageStream.Write(buffer.Array, buffer.Offset, result.Count);
                        }
                        else
                        {
                            _logger.LogError("WebSocket receive buffer array is null. Cannot process message.");
                            return; // Critical error, cannot proceed
                        }

                    } while (!result.EndOfMessage);

                    if (ct.IsCancellationRequested) break;

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var fullMessage = Encoding.UTF8.GetString(messageStream.ToArray());

                        // It's good practice to ensure HandleMessage does not block for too long.
                        // For very high frequency messages or long processing, consider a producer/consumer queue (e.g., System.Threading.Channels)
                        await HandleSingleMessageAsync(fullMessage, ct);
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    _logger.LogInformation("ProcessMessagesAsync cancelled.");
                    break;
                }
                catch (WebSocketException wsEx)
                {
                    // This often indicates a connection issue. The outer ExecuteAsync loop will handle reconnection.
                    _logger.LogError(wsEx, "WebSocketException in ProcessMessagesAsync. State: {State}. Error: {ErrorMessage}", _ws?.State, wsEx.Message);
                    break; // Exit loop, let ExecuteAsync handle reconnection
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message in ProcessMessagesAsync. Message processing will continue if possible.");
                    // Consider if you need to break here or if it's safe to continue the loop.
                    // If parsing errors are common, this might log too much.
                }
            }
            _logger.LogInformation("ProcessMessagesAsync stopped. WebSocket State: {State}", _ws?.State);
        }

        private async Task HandleSingleMessageAsync(string message, CancellationToken ct)
        {
            try
            {
                var obj = JObject.Parse(message);
                var encodedMessage = obj["message"]?.ToString();
                if (string.IsNullOrEmpty(encodedMessage))
                {
                    _logger.LogDebug("Received message without 'message' field or empty: {FullMessage}", message);
                    return;
                }

                var dataBytes = Convert.FromBase64String(encodedMessage);
                var update = PricingData.Parser.ParseFrom(dataBytes); // Assuming PricingData is your Protobuf message type

                _logger.LogInformation("Received update: {Symbol} - Price: {Price}, Change: {Change}, PercentChange: {PercentChange}%",
                    update.Id, update.Price, update.Change, update.ChangePercent);

                await HandleAlertsAsync(update, ct);
            }
            catch (JsonReaderException jex)
            {
                _logger.LogError(jex, "Failed to parse JSON from WebSocket message: {Message}", message);
            }
            catch (FormatException fex)
            {
                _logger.LogError(fex, "Failed to decode Base64 string from message: {Message}", message);
            }
            catch (Google.Protobuf.InvalidProtocolBufferException pex)
            {
                _logger.LogError(pex, "Failed to parse Protobuf message. Raw Base64: {Base64Data}", JObject.Parse(message)["message"]?.ToString()); // Log raw base64 if parsing fails
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error handling single message: {Message}", message);
            }
        }


        private async Task HandleAlertsAsync(PricingData update, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var thresholdTime = DateTime.UtcNow.AddSeconds(-ALERT_RATE_LIMIT_SECONDS);

                var alerts = await db.PriceAlerts
                    .Where(pa => pa.Stock.Symbol == update.Id
                        && ((pa.Direction == PriceAlertDirection.Above && (decimal)update.Price >= pa.Price)
                         || (pa.Direction == PriceAlertDirection.Below && (decimal)update.Price <= pa.Price))
                        && (pa.UpdatedAt == null || pa.UpdatedAt < thresholdTime))
                    .Include(pa => pa.Chat)
                    .Include(pa => pa.Stock)
                    .ToListAsync(ct);

                if (!alerts.Any())
                    return;

                _logger.LogInformation("Found {AlertCount} alerts to process for {Symbol} at price {Price}", alerts.Count, update.Id, update.Price);

                var stockCurrencySymbol = CurrencyHelper.GetCurrencySymbol(alerts.First().Stock.Currency); // Assuming all alerts for a stock share currency

                foreach (var alert in alerts)
                {
                    if (ct.IsCancellationRequested) break;

                    try
                    {
                        var dirEmoji = alert.Direction == PriceAlertDirection.Above ? "🚀" : "📉";
                        var dirText = alert.Direction == PriceAlertDirection.Above ? "Above" : "Below";

                        await _botClient.SendMessage(
                            chatId: alert.Chat.TelegramChatID,
                            text: $"🔔 *{alert.Stock.Symbol}*: {stockCurrencySymbol}{update.Price:N2} 🔔\n" + // :N2 for 2 decimal places
                                  $"{dirEmoji} Price {dirText} Target {stockCurrencySymbol}{alert.Price:N2} {dirEmoji}",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: ct
                        );

                        alert.AlertCount++;
                        alert.UpdatedAt = DateTime.UtcNow;

                        if (alert.AlertCount >= ALERT_MAX_COUNT_BEFORE_REMOVAL)
                        {
                            var otherAlertsForStockCount = await db.PriceAlerts
                                .CountAsync(pa => pa.StockID == alert.StockID && pa.ID != alert.ID, ct);

                            if (otherAlertsForStockCount == 0)
                            {
                                alert.Stock.IsWatched = false;
                                _logger.LogInformation("Unwatching stock {Symbol} as all alerts removed and it was the last one.", alert.Stock.Symbol);
                            }

                            db.PriceAlerts.Remove(alert);
                            _logger.LogInformation("Removed alert for {Symbol} after reaching max count. ChatID: {ChatId}", alert.Stock.Symbol, alert.Chat.TelegramChatID);

                            await _botClient.SendMessage(
                                chatId: alert.Chat.TelegramChatID,
                                text: $"🚫 *{alert.Stock.Symbol}* Alert ({dirText} {stockCurrencySymbol}{alert.Price:N2}) removed after {ALERT_MAX_COUNT_BEFORE_REMOVAL} notifications. 🚫",
                                parseMode: ParseMode.Markdown,
                                cancellationToken: ct
                            );
                        }
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested)
                    {
                        _logger.LogWarning("Alert processing for {Symbol} cancelled.", alert.Stock.Symbol);
                        break; // Exit loop if cancellation requested
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send Telegram alert for {Symbol} to ChatID {ChatId} or update DB. Alert ID: {AlertId}",
                            alert.Stock.Symbol, alert.Chat.TelegramChatID, alert.ID);
                        // Continue to the next alert
                    }
                }

                if (alerts.Any(a => db.Entry(a).State != EntityState.Detached)) // Check if there are any changes to save
                {
                    await db.SaveChangesAsync(ct);
                    _logger.LogDebug("Saved changes to DB after processing alerts for {Symbol}", update.Id);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogInformation("HandleAlertsAsync for {Symbol} cancelled.", update.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in HandleAlertsAsync for {Symbol}", update.Id);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StockPriceWebSocketListener.StopAsync called.");

            // This will signal the ExecuteAsync loop to stop.
            // The finally block in ExecuteAsync should handle closing the WebSocket.
            await base.StopAsync(cancellationToken);

            _logger.LogInformation("StockPriceWebSocketListener.StopAsync completed.");
        }
    }
}