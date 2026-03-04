using System.Linq.Expressions;

namespace SharedKernel;

public interface IReadRepository<TEntity>
    where TEntity : Entity<Guid>, IAggregateRoot
{
    Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task<TEntity?> FirstOrDefaultAsync(
        CancellationToken ct = default);
    
    Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> spec,
        CancellationToken ct = default);

    Task<List<TEntity>> ListAsync(
        CancellationToken ct = default);
    
    Task<List<TEntity>> ListAsync(
        ISpecification<TEntity> spec, 
        CancellationToken ct = default);

    Task<bool> AnyAsync(
        CancellationToken ct = default);
    
    Task<bool> AnyAsync(
        ISpecification<TEntity> spec, 
        CancellationToken ct = default);

    Task<int> CountAsync(
        CancellationToken ct = default);
    
    Task<int> CountAsync(
        ISpecification<TEntity> spec, 
        CancellationToken ct = default);
}