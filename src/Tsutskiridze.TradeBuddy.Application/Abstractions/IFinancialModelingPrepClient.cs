using Tsutskiridze.TradeBuddy.Application.Dtos;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions
{
    public interface IFinancialModelingPrepClient
    {
        Task<StockQuote?> GetStockQuote(string symbol);
    }
}
