using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

public class EfRepository<T> : IRepository<T> where T : IAggregateRoot
{
    protected readonly AppDbContext _db;

    public EfRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TEntity?> GetById<TEntity, TId>(TId id, CancellationToken ct = default)
        where TEntity : Entity<TId>, IAggregateRoot where TId : notnull
    {
        return await _db.Set<TEntity>().FirstOrDefaultAsync(x => x.Id.Equals(id), ct);
    }

    public async Task Add<TEntity, TId>(TEntity entity, CancellationToken ct = default)
        where TEntity : Entity<TId>, IAggregateRoot where TId : notnull
    {
        await _db.Set<TEntity>().AddAsync(entity, ct);
    }

    public async Task Remove<TEntity, TId>(TEntity entity, CancellationToken ct = default)
        where TEntity : Entity<TId>, IAggregateRoot where TId : notnull
    {
        _db.Set<TEntity>().Remove(entity);
        await Task.CompletedTask;
    }
}