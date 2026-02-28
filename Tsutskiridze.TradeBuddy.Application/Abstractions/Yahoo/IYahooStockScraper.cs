using Tsutskiridze.TradeBuddy.Core.Entities;

namespace Tsutskiridze.TradeBuddy.Application.Interfaces.Yahoo
{
    public interface IYahooStockScraper
    {
        Task<bool> StockSymbolExits(string symbol);
        Task<List<StockDayPrice>> GetStockPrevDaysClosePrices(string symbol, int? days = null);
        Task<StockOverview?> GetStockOverview(string symbol);
        Task<AnnualReport?> GetStockLastAnnualReport(string symbol);
        Task<StockQuote?> GetStockQuote(string symbol);
    }
}
