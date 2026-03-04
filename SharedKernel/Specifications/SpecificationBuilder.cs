using System.Linq.Expressions;

namespace SharedKernel.Specifications;

public class SpecificationBuilder<TEntity> : ISpecificationBuilder<TEntity> where TEntity : Entity<Guid>, IAggregateRoot
{
    public Specification<TEntity> Specification { get; }

    public SpecificationBuilder(Specification<TEntity> specification)
    {
        Specification = specification;
    }

    public SpecificationBuilder<TEntity> Where(Expression<Func<TEntity, bool>> criteria)
    {
        ((List<Expression<Func<TEntity, bool>>>)Specification.Criterias).Add(criteria);
        return this;
    }

    public SpecificationBuilder<TEntity> Include(Expression<Func<TEntity, object>> include)
    {
        ((List<Expression<Func<TEntity, object>>>)Specification.Includes).Add(include);
        return this;
    }

    public SpecificationBuilder<TEntity> Skip(int count)
    {
        Specification.Skip = count;
        return this;
    }

    public SpecificationBuilder<TEntity> Take(int range)
    {
        Specification.Take = range;
        return this;
    }

    public SpecificationBuilder<TEntity> OrderBy(
        Expression<Func<TEntity, object?>> keySelector)
    {
        Specification.OrderByDescending = null;
        Specification.OrderBy = keySelector;
        return this;
    }

    public SpecificationBuilder<TEntity> OrderByDescending(
        Expression<Func<TEntity, object?>> keySelector)
    {
        Specification.OrderBy = null;
        Specification.OrderByDescending = keySelector;
        return this;
    }
}