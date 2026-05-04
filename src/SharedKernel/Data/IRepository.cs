namespace SharedKernel.Data;

public interface IRepository<TEntity> : IRepository<TEntity, Guid>, IReadRepository<TEntity>
    where TEntity : Entity<Guid>, IAggregateRoot;

public interface IRepository<TEntity, TId> : IReadRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);

    void Update(TEntity entity);
    
    Task<TEntity?> GetByIdForUpdateAsync(TId id, CancellationToken ct = default);
    Task LockByIdAsync(TId id, CancellationToken ct = default);
}
