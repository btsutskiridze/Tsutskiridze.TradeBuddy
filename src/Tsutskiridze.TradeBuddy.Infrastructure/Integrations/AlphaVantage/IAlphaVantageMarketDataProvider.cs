using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.AlphaVantage
{
    public interface IAlphaVantageMarketDataProvider
    {
        Task<AnnualReportDto?> GetStockLastAnnualReport(string symbol);
        Task<StockOverviewDto?> GetStockOverview(string symbol);
        Task<List<StockDayPriceDto>> GetStockPrevDaysClosePrices(string symbol, int? days = null);
    }
}
