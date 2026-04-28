namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Streaming.Abstractions
{
    public interface ISubscriptionManager
    {
        /// <summary>Load initial watched symbols and push to the transport.</summary>
        Task InitializeAsync(CancellationToken ct);
        Task SubscribeStockPriceAsync(string symbol, CancellationToken ct);
        Task UnsubscribeStockPriceAsync(string symbol, CancellationToken ct);
    }
}
