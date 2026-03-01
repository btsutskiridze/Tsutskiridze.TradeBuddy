using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

public class EfReadRepository<TEntity, TId> : IReadRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<TEntity> Set;

    public EfReadRepository(AppDbContext db)
    {
        Db = db;
        Set = db.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> query = Set;

        if (asNoTracking)
            query = query.AsNoTracking();

        if (queryShaper is not null)
            query = queryShaper(query);

        return await query.FirstOrDefaultAsync(x => x.Id.Equals(id), ct);
    }
    
    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> query = Set;

        if (asNoTracking)
            query = query.AsNoTracking();

        query = query.Where(predicate);

        if (queryShaper is not null)
            query = queryShaper(query);

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<List<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        IQueryable<TEntity> query = Set;

        if (asNoTracking)
            query = query.AsNoTracking();

        if (predicate is not null)
            query = query.Where(predicate);

        if (queryShaper is not null)
            query = queryShaper(query);

        return await query.ToListAsync(ct);
    }

    public async Task<List<TEntity>> ListByIdsAsync(
        IReadOnlyCollection<TId> ids,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return [];

        IQueryable<TEntity> query = Set;

        if (asNoTracking)
            query = query.AsNoTracking();

        query = query.Where(x => ids.Contains(x.Id));

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