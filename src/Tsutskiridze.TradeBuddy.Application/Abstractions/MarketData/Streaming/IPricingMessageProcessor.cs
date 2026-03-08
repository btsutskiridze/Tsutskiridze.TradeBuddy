namespace Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Streaming
{
    public interface IPricingMessageProcessor
    {
        Task ProcessAsync(string json, CancellationToken ct);
    }
}
