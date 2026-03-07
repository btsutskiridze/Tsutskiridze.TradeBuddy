using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData
{
    public interface IFinancialModelingPrepQuoteProvider
    {
        Task<StockQuoteDto?> GetStockQuote(string symbol);
    }
}
