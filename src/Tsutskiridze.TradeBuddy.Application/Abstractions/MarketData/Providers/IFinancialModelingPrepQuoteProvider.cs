using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers
{
    public interface IFinancialModelingPrepQuoteProvider
    {
        Task<StockQuoteDto?> GetStockQuote(string symbol);
    }
}
