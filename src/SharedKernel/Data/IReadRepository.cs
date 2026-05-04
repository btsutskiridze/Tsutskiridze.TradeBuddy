using SharedKernel.Specifications;

namespace SharedKernel.Data;

public interface IReadRepository<TEntity> : IReadRepository<TEntity, Guid>
    where TEntity : Entity<Guid>, IAggregateRoot;

public interface IReadRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken ct = default);

    Task<TEntity?> FirstOrDefaultAsync(
        CancellationToken ct = default);

    Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity, TId> spec,
        CancellationToken ct = default);

    Task<TResult?> FirstOrDefaultAsync<TResult>(
        ISpecification<TEntity, TId, TResult> spec,
        CancellationToken ct = default);

    Task<List<TEntity>> ListAsync(
        CancellationToken ct = default);

    Task<List<TEntity>> ListAsync(
        ISpecification<TEntity, TId> spec,
        CancellationToken ct = default);

    Task<List<TResult>> ListAsync<TResult>(
        ISpecification<TEntity, TId, TResult> spec,
        CancellationToken ct = default);

    Task<bool> AnyAsync(
        CancellationToken ct = default);

    Task<bool> AnyAsync(
        ISpecification<TEntity, TId> spec,
        CancellationToken ct = default);

    Task<bool> AnyAsync<TResult>(
        ISpecification<TEntity, TId, TResult> spec,
        CancellationToken ct = default);

    Task<int> CountAsync(
        CancellationToken ct = default);

    Task<int> CountAsync(
        ISpecification<TEntity, TId> spec,
        CancellationToken ct = default);

    Task<int> CountAsync<TResult>(
        ISpecification<TEntity, TId, TResult> spec,
        CancellationToken ct = default);
}
