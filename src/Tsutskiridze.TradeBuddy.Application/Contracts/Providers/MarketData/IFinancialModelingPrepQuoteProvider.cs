using Tsutskiridze.TradeBuddy.Application.Contracts.MarketData;

namespace Tsutskiridze.TradeBuddy.Application.Contracts.Providers.MarketData
{
    public interface IFinancialModelingPrepQuoteProvider
    {
        Task<StockQuoteDto?> GetStockQuote(string symbol);
    }
}
