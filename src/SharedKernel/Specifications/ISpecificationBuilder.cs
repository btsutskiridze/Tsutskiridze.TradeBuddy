using System.Linq.Expressions;

namespace SharedKernel.Specifications;

public interface ISpecificationBuilder<TEntity, TId, TResult> : ISpecificationBuilder<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    new Specification<TEntity, TId, TResult> Specification { get; }
    ISpecificationBuilder<TEntity, TId, TResult> Select(Expression<Func<TEntity, TResult>>? selector);
}

public interface ISpecificationBuilder<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    Specification<TEntity, TId> Specification { get; }

    ISpecificationBuilder<TEntity, TId> Where(Expression<Func<TEntity, bool>> criteria);

    ISpecificationBuilder<TEntity, TId> Include(Expression<Func<TEntity, object>> include);

    ISpecificationBuilder<TEntity, TId> Skip(int count);

    ISpecificationBuilder<TEntity, TId> Take(int range);

    ISpecificationBuilder<TEntity, TId> OrderBy(Expression<Func<TEntity, object?>> keySelector);

    ISpecificationBuilder<TEntity, TId> OrderByDescending(Expression<Func<TEntity, object?>> keySelector);
}