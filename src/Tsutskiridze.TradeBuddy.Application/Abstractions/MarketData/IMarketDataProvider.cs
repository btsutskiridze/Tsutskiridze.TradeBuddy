using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData
{
    public interface IMarketDataProvider
    {
        Task<bool> StockSymbolExists(string symbol);
        Task<List<StockDayPrice>> GetStockPrevDaysClosePrices(string symbol, int? days = null);
        Task<StockOverview?> GetStockOverview(string symbol);
        Task<AnnualReport?> GetStockLastAnnualReport(string symbol);
        Task<StockQuote?> GetStockQuote(string symbol);
        Task<IReadOnlyList<MarketCandle>> GetDailyCandles(string symbol, DateOnly from, DateOnly to, CancellationToken ct);
    }
}
