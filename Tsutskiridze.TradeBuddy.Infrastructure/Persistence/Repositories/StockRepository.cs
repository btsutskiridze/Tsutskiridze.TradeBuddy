using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Core.Repositories;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

public class StockRepository:EfRepository<Stock>,IStockRepository
{
    public StockRepository(AppDbContext db) : base(db)
    {
    }

    public async Task<Stock?> GetBySymbol(string symbol, CancellationToken ct = default)
    {
        return _db.Stocks.FirstOrDefault(x => x.Symbol == symbol);
    }
}