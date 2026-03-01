using Microsoft.EntityFrameworkCore;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Core.Enums;
using Tsutskiridze.TradeBuddy.Core.Repositories;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

public class PriceAlertReadRepository : IPriceAlertReadRepository
{
    private readonly AppDbContext _db;

    public PriceAlertReadRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PriceAlert>> GetTriggeredAlerts(string symbol, decimal price,
        CancellationToken ct = default)
    {
        return await (
            from pa in _db.PriceAlerts
            join s in _db.Stocks on pa.StockId equals s.Id
            where s.Symbol == symbol && pa.IsTriggered(price)
            select pa
        ).ToListAsync(ct);
    }
}