using SharedKernel;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;

namespace Tsutskiridze.TradeBuddy.Core.Repositories;

public interface IStockRepository : IRepository<Stock>
{
    Task<Stock?> GetBySymbol(string symbol, CancellationToken ct = default);
}