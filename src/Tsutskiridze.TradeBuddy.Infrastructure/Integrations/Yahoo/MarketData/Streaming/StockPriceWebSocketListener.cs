using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Streaming.Abstractions;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Streaming
{
    public sealed class StockPriceWebSocketListener : BackgroundService
    {
        private readonly IMarketDataTransportClient _transport;
        private readonly ISubscriptionManager _subs;
        private readonly IPricingMessageProcessor _processor;
        private readonly ILogger<StockPriceWebSocketListener> _log;

        public StockPriceWebSocketListener(
            IMarketDataTransportClient transport,
            ISubscriptionManager subs,
            IPricingMessageProcessor processor,
            ILogger<StockPriceWebSocketListener> log)
        {
            _transport = transport;
            _subs = subs;
            _processor = processor;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _log.LogInformation("Starting WS orchestrator");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await _transport.ConnectAsync(ct);

                    await _subs.InitializeAsync(ct);

                    await foreach (var msg in _transport.ReceiveAsync(ct))
                    {
                        await _processor.ProcessAsync(msg, ct);
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Orchestrator error, reconnecting in 10s");
                    await Task.Delay(TimeSpan.FromSeconds(10), ct);
                }
            }
        }
    }
}
