namespace Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;

public interface IMarketDataListener
{
    Task SubscribeStockPriceAsync(string symbol, CancellationToken ct);
    Task UnsubscribeStockPriceAsync(string symbol, CancellationToken ct);
}