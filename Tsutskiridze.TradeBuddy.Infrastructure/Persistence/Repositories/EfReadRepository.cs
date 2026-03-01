using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

public class EfReadRepository<TEntity> : IReadRepository<TEntity>
    where TEntity : Entity<Guid>, IAggregateRoot
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<TEntity> Set;

    protected virtual bool UseNoTracking => true;

    public EfReadRepository(AppDbContext db)
    {
        Db = db;
        Set = db.Set<TEntity>();
    }

    protected IQueryable<TEntity> Query()
    {
        IQueryable<TEntity> query = Set;
        return UseNoTracking ? query.AsNoTracking() : query;
    }

    public async Task<TEntity?> GetByIdAsync(
        Guid id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        CancellationToken ct = default)
    {
        var query = Query();

        if (queryShaper is not null)
            query = queryShaper(query);

        return await query.FirstOrDefaultAsync(x => x.Id.Equals(id), ct);
    }
    
    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        CancellationToken ct = default)
    {
        var query = Query().Where(predicate);

        if (queryShaper is not null)
            query = queryShaper(query);

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<List<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        CancellationToken ct = default)
    {
        var query = Query();

        if (predicate is not null)
            query = query.Where(predicate);

        if (queryShaper is not null)
            query = queryShaper(query);

        return await query.ToListAsync(ct);
    }

    public async Task<List<TEntity>> ListByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return [];

        var query = Query().Where(x => ids.Contains(x.Id));

        if (queryShaper is not null)
            query = queryShaper(query);

        return await query.ToListAsync(ct);
    }
    

    public Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
    {
        return Set.AnyAsync(predicate, ct);
    }

    public Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> query = Set;

        if (predicate is not null)
            query = query.Where(predicate);

        return query.CountAsync(ct);
    }
}