using Tsutskiridze.TradeBuddy.Core.Entities;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions
{
    public interface IAlphaVantageClient
    {
        Task<AnnualReport?> GetStockLastAnnualReport(string symbol);
        Task<StockOverview?> GetStockOverview(string symbol);
        Task<List<StockDayPrice>> GetStockPrevDaysClosePrices(string symbol, int? days = null);
    }
}
