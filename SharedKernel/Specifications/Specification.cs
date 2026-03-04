using System.Linq.Expressions;

namespace SharedKernel.Specifications;

public abstract class Specification<TEntity> : ISpecification<TEntity> where TEntity : Entity<Guid>, IAggregateRoot
{
    protected ISpecificationBuilder<TEntity> Query { get; }

    protected Specification()
    {
        Query = new SpecificationBuilder<TEntity>(this);
    }

    public IEnumerable<Expression<Func<TEntity, bool>>> Criterias { get; } = [];
    public IEnumerable<Expression<Func<TEntity, object>>> Includes { get; } = [];
    public Expression<Func<TEntity, object?>>? OrderBy { get; internal set; }
    public Expression<Func<TEntity, object?>>? OrderByDescending { get; internal set; }
    public int? Skip { get; internal set; }
    public int? Take { get; internal set; }
}