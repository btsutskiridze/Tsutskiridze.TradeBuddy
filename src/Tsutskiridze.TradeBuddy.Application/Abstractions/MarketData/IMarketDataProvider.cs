using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData
{
    public interface IMarketDataProvider
    {
        Task<bool> StockSymbolExists(string symbol);
        Task<List<StockDayPriceDto>> GetStockPrevDaysClosePrices(string symbol, int? days = null);
        Task<StockOverviewDto?> GetStockOverview(string symbol);
        Task<AnnualReportDto?> GetStockLastAnnualReport(string symbol);
        Task<StockQuoteDto?> GetStockQuote(string symbol);
    }
}
