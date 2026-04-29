using System.Linq.Expressions;

namespace SharedKernel.Specifications;

public class SpecificationBuilder<TEntity, TResult> : SpecificationBuilder<TEntity>,
    ISpecificationBuilder<TEntity, TResult> where TEntity : Entity<Guid>, IAggregateRoot
{
    public new Specification<TEntity, TResult> Specification { get; }

    public SpecificationBuilder(Specification<TEntity, TResult> specification) : base(specification)
    {
        Specification = specification;
    }

    public ISpecificationBuilder<TEntity, TResult> Select(Expression<Func<TEntity, TResult>>? selector)
    {
        Specification.Selector = selector;
        return this;
    }
}

public class SpecificationBuilder<TEntity> : ISpecificationBuilder<TEntity> where TEntity : Entity<Guid>, IAggregateRoot
{
    public Specification<TEntity> Specification { get; }

    public SpecificationBuilder(Specification<TEntity> specification)
    {
        Specification = specification;
    }

    public ISpecificationBuilder<TEntity> Where(Expression<Func<TEntity, bool>> criteria)
    {
        Specification.AddCriteria(criteria);
        return this;
    }

    public ISpecificationBuilder<TEntity> Include(Expression<Func<TEntity, object>> include)
    {
        Specification.AddInclude(include);
        return this;
    }

    public ISpecificationBuilder<TEntity> Skip(int count)
    {
        Specification.Skip = count;
        return this;
    }

    public ISpecificationBuilder<TEntity> Take(int range)
    {
        Specification.Take = range;
        return this;
    }

    public ISpecificationBuilder<TEntity> OrderBy(
        Expression<Func<TEntity, object?>> keySelector)
    {
        Specification.OrderByDescending = null;
        Specification.OrderBy = keySelector;
        return this;
    }

    public ISpecificationBuilder<TEntity> OrderByDescending(
        Expression<Func<TEntity, object?>> keySelector)
    {
        Specification.OrderBy = null;
        Specification.OrderByDescending = keySelector;
        return this;
    }
}