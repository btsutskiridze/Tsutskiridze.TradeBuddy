using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Specifications;

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

    protected IQueryable<TEntity> Query(ISpecification<TEntity>? spec = null)
    {
        IQueryable<TEntity> query = Set;

        if (UseNoTracking)
            query.AsNoTracking();

        if (spec is not null)
            query = SpecificationEvaluator<TEntity>.ShapeQuery(spec, query);

        return query;
    }

    protected IQueryable<TResult> Query<TResult>(ISpecification<TEntity, TResult> spec)
    {
        IQueryable<TEntity> startQuery = Set;

        if (UseNoTracking)
            startQuery.AsNoTracking();

        var query = SpecificationEvaluator<TEntity>.ShapeQuery(spec, startQuery);

        return query;
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await Set.FindAsync([id], cancellationToken: ct);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(CancellationToken ct = default)
    {
        return await Query().FirstOrDefaultAsync(ct);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> spec, CancellationToken ct = default)
    {
        return await Query(spec).FirstOrDefaultAsync(ct);
    }

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<TEntity, TResult> spec, CancellationToken ct = default)
    {
        return await Query(spec).FirstOrDefaultAsync(ct);
    }

    public async Task<List<TEntity>> ListAsync(CancellationToken ct = default)
    {
        return await Query().ToListAsync(ct);
    }

    public async Task<List<TEntity>> ListAsync(ISpecification<TEntity> spec, CancellationToken ct = default)
    {
        return await Query(spec).ToListAsync(ct);
    }

    public async Task<List<TResult>> ListAsync<TResult>(ISpecification<TEntity, TResult> spec, CancellationToken ct = default)
    {
        return await Query(spec).ToListAsync(ct);
    }

    public async Task<bool> AnyAsync(CancellationToken ct = default)
    {
        return await Query().AnyAsync(ct);
    }

    public async Task<bool> AnyAsync(ISpecification<TEntity> spec, CancellationToken ct = default)
    {
        return await Query(spec).AnyAsync(ct);
    }

    public async Task<bool> AnyAsync<TResult>(ISpecification<TEntity, TResult> spec, CancellationToken ct = default)
    {
        return await Query(spec).AnyAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await Query().CountAsync(ct);
    }

    public async Task<int> CountAsync(ISpecification<TEntity> spec, CancellationToken ct = default)
    {
        return await Query(spec).CountAsync(ct);
    }

    public async Task<int> CountAsync<TResult>(ISpecification<TEntity, TResult> spec, CancellationToken ct = default)
    {
        return await Query(spec).CountAsync(ct);
    }
}