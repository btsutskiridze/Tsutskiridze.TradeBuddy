using Tsutskiridze.TradeBuddy.Core.Entities;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions
{
    public interface IFinancialModelingPrepClient
    {
        Task<StockQuote?> GetStockQuote(string symbol);
    }
}
