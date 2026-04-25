namespace SharedKernel;

public interface IRepository<TEntity> : IReadRepository<TEntity>
    where TEntity : Entity<Guid>, IAggregateRoot
{
    Task<TEntity?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);

    void Update(TEntity entity);
}