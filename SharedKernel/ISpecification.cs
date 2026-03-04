using System.Linq.Expressions;

namespace SharedKernel;

public interface ISpecification<TEntity> where TEntity : Entity<Guid>, IAggregateRoot
{
    IQueryable<TEntity> GetQuery(IQueryable<TEntity> query);
}