using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Streaming.Abstractions;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData;

public class MarketDataListener : IMarketDataListener
{
    private readonly ISubscriptionManager _manager;

    public MarketDataListener(ISubscriptionManager manager)
    {
        _manager = manager;
    }

    public async Task SubscribeStockPriceAsync(string symbol, CancellationToken ct)
    {
        await _manager.SubscribeStockPriceAsync(symbol, ct);
    }

    public async Task UnsubscribeStockPriceAsync(string symbol, CancellationToken ct)
    {
        await _manager.UnsubscribeStockPriceAsync(symbol, ct);
    }
}