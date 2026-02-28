using Tsutskiridze.TradeBuddy.Core.Entities;

namespace Tsutskiridze.TradeBuddy.Application.Interfaces
{
    public interface IFinancialModelingPrepClient
    {
        Task<StockQuote?> GetStockQuote(string symbol);
    }
}
