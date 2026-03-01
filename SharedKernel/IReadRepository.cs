using System.Linq.Expressions;

namespace SharedKernel;

public interface IReadRepository<TEntity, in TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        bool asNoTracking = true,
        CancellationToken ct = default);

    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        bool asNoTracking = true,
        CancellationToken ct = default);

    Task<List<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        bool asNoTracking = true,
        CancellationToken ct = default);
    
    Task<List<TEntity>> ListByIdsAsync(
        IReadOnlyCollection<TId> ids,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryShaper = null,
        bool asNoTracking = true,
        CancellationToken ct = default);

    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default);

    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken ct = default);
}