namespace Tsutskiridze.TradeBuddy.Application.Abstractions.StockMarket
{
    public interface IPricingMessageProcessor
    {
        Task ProcessAsync(string json, CancellationToken ct);
    }
}
