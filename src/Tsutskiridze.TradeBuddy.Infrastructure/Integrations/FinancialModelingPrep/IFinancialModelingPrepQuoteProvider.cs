using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep
{
    public interface IFinancialModelingPrepQuoteProvider
    {
        Task<StockQuoteDto?> GetStockQuote(string symbol);
    }
}
