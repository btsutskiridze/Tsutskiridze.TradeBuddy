using Tsutskiridze.TradeBuddy.Application.Contracts.MarketData;

namespace Tsutskiridze.TradeBuddy.Application.Contracts.Providers.MarketData
{
    public interface IAlphaVantageMarketDataProvider
    {
        Task<AnnualReportDto?> GetStockLastAnnualReport(string symbol);
        Task<StockOverviewDto?> GetStockOverview(string symbol);
        Task<List<StockDayPriceDto>> GetStockPrevDaysClosePrices(string symbol, int? days = null);
    }
}
