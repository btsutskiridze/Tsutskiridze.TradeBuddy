using System.Linq.Expressions;

namespace SharedKernel;

public abstract class Specification<TEntity> : ISpecification<TEntity> where TEntity : Entity<Guid>, IAggregateRoot
{
    private List<Func<IQueryable<TEntity>, IQueryable<TEntity>>> Queries { get; } = [];

    protected void Query(Func<IQueryable<TEntity>, IQueryable<TEntity>> queryStep)
    {
        Queries.Add(queryStep);
    }

    public IQueryable<TEntity> GetQuery(IQueryable<TEntity> query)
    {
        return Queries.Aggregate(query, (current, step) => step(current));
    }
}