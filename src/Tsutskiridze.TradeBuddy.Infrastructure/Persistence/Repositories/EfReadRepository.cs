using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Data;
using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

public class EfReadRepository<TEntity> : EfReadRepository<TEntity, Guid>, IReadRepository<TEntity>
    where TEntity : Entity<Guid>, IAggregateRoot
{
    public EfReadRepository(AppDbContext db) : base(db)
    {
    }
}

public class EfReadRepository<TEntity, TId> : IReadRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<TEntity> Set;

    protected virtual bool UseNoTracking => true;

    public EfReadRepository(AppDbContext db)
    {
        Db = db;
        Set = db.Set<TEntity>();
    }

    protected IQueryable<TEntity> Query(ISpecification<TEntity, TId>? spec = null)
    {
        IQueryable<TEntity> query = Set;

        if (UseNoTracking)
            query = query.AsNoTracking();

        if (spec is not null)
            query = SpecificationEvaluator<TEntity, TId>.ShapeQuery(spec, query);

        return query;
    }

    protected IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate)
    {
        IQueryable<TEntity> query = Set;

        if (UseNoTracking)
            query = query.AsNoTracking();

        query = query.Where(predicate);

        return query;
    }

    protected IQueryable<TResult> Query<TResult>(ISpecification<TEntity, TId, TResult> spec)
    {
        IQueryable<TEntity> startQuery = Set;

        if (UseNoTracking)
            startQuery = startQuery.AsNoTracking();

        var query = SpecificationEvaluator<TEntity, TId>.ShapeQuery(spec, startQuery);

        return query;
    }

    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default)
    {
        return await Query(e => e.Id.Equals(id)).FirstOrDefaultAsync(ct);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(CancellationToken ct = default)
    {
        return await Query().FirstOrDefaultAsync(ct);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity, TId> spec, CancellationToken ct = default)
    {
        return await Query(spec).FirstOrDefaultAsync(ct);
    }

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<TEntity, TId, TResult> spec,
        CancellationToken ct = default)
    {
        return await Query(spec).FirstOrDefaultAsync(ct);
    }

    public async Task<List<TEntity>> ListAsync(CancellationToken ct = default)
    {
        return await Query().ToListAsync(ct);
    }

    public async Task<List<TEntity>> ListAsync(ISpecification<TEntity, TId> spec, CancellationToken ct = default)
    {
        return await Query(spec).ToListAsync(ct);
    }

    public async Task<List<TResult>> ListAsync<TResult>(ISpecification<TEntity, TId, TResult> spec,
        CancellationToken ct = default)
    {
        return await Query(spec).ToListAsync(ct);
    }

    public async Task<bool> AnyAsync(CancellationToken ct = default)
    {
        return await Query().AnyAsync(ct);
    }

    public async Task<bool> AnyAsync(ISpecification<TEntity, TId> spec, CancellationToken ct = default)
    {
        return await Query(spec).AnyAsync(ct);
    }

    public async Task<bool> AnyAsync<TResult>(ISpecification<TEntity, TId, TResult> spec,
        CancellationToken ct = default)
    {
        return await Query(spec).AnyAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await Query().CountAsync(ct);
    }

    public async Task<int> CountAsync(ISpecification<TEntity, TId> spec, CancellationToken ct = default)
    {
        return await Query(spec).CountAsync(ct);
    }

    public async Task<int> CountAsync<TResult>(ISpecification<TEntity, TId, TResult> spec,
        CancellationToken ct = default)
    {
        return await Query(spec).CountAsync(ct);
    }
}