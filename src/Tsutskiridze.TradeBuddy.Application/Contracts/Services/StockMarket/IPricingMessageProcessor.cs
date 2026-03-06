namespace Tsutskiridze.TradeBuddy.Application.Contracts.Services.StockMarket
{
    public interface IPricingMessageProcessor
    {
        Task ProcessAsync(string json, CancellationToken ct);
    }
}
