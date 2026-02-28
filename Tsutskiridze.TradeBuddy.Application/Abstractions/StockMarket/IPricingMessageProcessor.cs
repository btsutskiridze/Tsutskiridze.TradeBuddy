namespace Tsutskiridze.TradeBuddy.Application.Interfaces.StockMarket
{
    public interface IPricingMessageProcessor
    {
        Task ProcessAsync(string json, CancellationToken ct);
    }
}
