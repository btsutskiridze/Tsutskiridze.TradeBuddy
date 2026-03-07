using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

public class EfRepository<TEntity> : EfReadRepository<TEntity>, IRepository<TEntity>
    where TEntity : Entity<Guid>, IAggregateRoot
{
    protected override bool UseNoTracking => false;

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