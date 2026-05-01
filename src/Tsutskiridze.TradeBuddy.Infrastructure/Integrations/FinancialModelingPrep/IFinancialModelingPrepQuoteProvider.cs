using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep
{
    public interface IFinancialModelingPrepQuoteProvider
    {
        Task<StockQuote?> GetStockQuote(string symbol);
    }
}
