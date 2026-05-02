using Microsoft.EntityFrameworkCore;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

public class TradeStrategyRepository(AppDbContext db) : ITradeStrategyRepository
{
    public async Task<TradeStrategy> AddAsync(TradeStrategy strategy, CancellationToken ct = default)
    {
        await db.TradeStrategies.AddAsync(strategy, ct);
        return strategy;
    }

    public async Task<TradeStrategy?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.TradeStrategies.FindAsync([id], cancellationToken: ct);
    }
    
    public async Task<List<TradeStrategy>> ListByChatIdAsync(Guid chatId, CancellationToken ct = default)
    {
        return await db.TradeStrategies
            .AsNoTracking()
            .Where(x => x.ChatId == chatId)
            .OrderBy(x => x.Id)
            .ToListAsync(ct);
    }

    public void Remove(TradeStrategy strategy)
    {
        db.TradeStrategies.Remove(strategy);
    }
}
