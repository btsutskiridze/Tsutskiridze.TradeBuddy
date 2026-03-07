using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;

namespace Tsutskiridze.TradeBuddy.Application.DTOs.Providers.MarketData
{
    public interface IFinancialModelingPrepQuoteProvider
    {
        Task<StockQuoteDto?> GetStockQuote(string symbol);
    }
}
