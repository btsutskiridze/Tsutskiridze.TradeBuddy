using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

public class EfRepository<TEntity, TId> : EfReadRepository<TEntity, TId>, IRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    public EfRepository(AppDbContext db) : base(db)
    {
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await Set.AddAsync(entity, ct);
        return entity;
    }

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        => Set.AddRangeAsync(entities, ct);

    public void Remove(TEntity entity)
        => Set.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities)
        => Set.RemoveRange(entities);

    public void Update(TEntity entity)
        => Set.Update(entity);
}