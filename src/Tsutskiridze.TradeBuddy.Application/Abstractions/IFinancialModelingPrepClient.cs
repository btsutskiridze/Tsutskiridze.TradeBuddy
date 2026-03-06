using Tsutskiridze.TradeBuddy.Application.Contracts.MarketData;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions
{
    public interface IFinancialModelingPrepClient
    {
        Task<StockQuoteDto?> GetStockQuote(string symbol);
    }
}
