using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.AlphaVantage
{
    public interface IAlphaVantageMarketDataProvider
    {
        Task<AnnualReport?> GetStockLastAnnualReport(string symbol);
        Task<StockOverview?> GetStockOverview(string symbol);
        Task<List<StockDayPrice>> GetStockPrevDaysClosePrices(string symbol, int? days = null);
    }
}
