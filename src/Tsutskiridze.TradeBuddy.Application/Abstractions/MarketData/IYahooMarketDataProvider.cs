using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData
{
    public interface IYahooMarketDataProvider
    {
        Task<bool> StockSymbolExits(string symbol);
        Task<List<StockDayPriceDto>> GetStockPrevDaysClosePrices(string symbol, int? days = null);
        Task<StockOverviewDto?> GetStockOverview(string symbol);
        Task<AnnualReportDto?> GetStockLastAnnualReport(string symbol);
        Task<StockQuoteDto?> GetStockQuote(string symbol);
    }
}
