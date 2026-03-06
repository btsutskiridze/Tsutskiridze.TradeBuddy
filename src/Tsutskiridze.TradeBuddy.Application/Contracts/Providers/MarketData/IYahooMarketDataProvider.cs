using Tsutskiridze.TradeBuddy.Application.Contracts.MarketData;

namespace Tsutskiridze.TradeBuddy.Application.Contracts.Providers.MarketData
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
