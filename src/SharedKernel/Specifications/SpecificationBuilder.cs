using System.Linq.Expressions;

namespace SharedKernel.Specifications;

public class SpecificationBuilder<TEntity, TId, TResult> : SpecificationBuilder<TEntity, TId>,
    ISpecificationBuilder<TEntity, TId, TResult>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    public new Specification<TEntity, TId, TResult> Specification { get; }

    public SpecificationBuilder(Specification<TEntity, TId, TResult> specification) : base(specification)
    {
        Specification = specification;
    }

    public ISpecificationBuilder<TEntity, TId, TResult> Select(Expression<Func<TEntity, TResult>>? selector)
    {
        Specification.Selector = selector;
        return this;
    }
}

public class SpecificationBuilder<TEntity, TId> : ISpecificationBuilder<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    public Specification<TEntity, TId> Specification { get; }

    public SpecificationBuilder(Specification<TEntity, TId> specification)
    {
        Specification = specification;
    }

    public ISpecificationBuilder<TEntity, TId> Where(Expression<Func<TEntity, bool>> criteria)
    {
        Specification.AddCriteria(criteria);
        return this;
    }

    public ISpecificationBuilder<TEntity, TId> Include(Expression<Func<TEntity, object>> include)
    {
        Specification.AddInclude(include);
        return this;
    }

    public ISpecificationBuilder<TEntity, TId> Skip(int count)
    {
        Specification.Skip = count;
        return this;
    }

    public ISpecificationBuilder<TEntity, TId> Take(int range)
    {
        Specification.Take = range;
        return this;
    }

    public ISpecificationBuilder<TEntity, TId> OrderBy(Expression<Func<TEntity, object?>> keySelector)
    {
        Specification.OrderByDescending = null;
        Specification.OrderBy = keySelector;
        return this;
    }

    public ISpecificationBuilder<TEntity, TId> OrderByDescending(Expression<Func<TEntity, object?>> keySelector)
    {
        Specification.OrderBy = null;
        Specification.OrderByDescending = keySelector;
        return this;
    }
}