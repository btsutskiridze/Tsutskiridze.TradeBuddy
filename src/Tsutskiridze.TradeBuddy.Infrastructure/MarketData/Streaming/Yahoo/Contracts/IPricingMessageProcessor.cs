namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Streaming.Yahoo.Contracts
{
    public interface IPricingMessageProcessor
    {
        Task ProcessAsync(string json, CancellationToken ct);
    }
}
