namespace SharedKernel;

public interface IRepository<TAggregate> where TAggregate : IAggregateRoot
{
    Task<TEntity?> GetById<TEntity, TId>(TId id, CancellationToken ct = default) where TEntity : Entity<TId>, IAggregateRoot where TId : notnull;
    Task Add<TEntity, TId>(TEntity entity, CancellationToken ct = default) where TEntity : Entity<TId>, IAggregateRoot where TId : notnull;
    Task Remove<TEntity, TId>(TEntity entity, CancellationToken ct = default) where TEntity : Entity<TId>, IAggregateRoot where TId : notnull;
}