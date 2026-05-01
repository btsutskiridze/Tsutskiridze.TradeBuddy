using System.Linq.Expressions;

namespace SharedKernel.Specifications;

public interface ISpecificationBuilder<TEntity, TResult> : ISpecificationBuilder<TEntity>
    where TEntity : Entity<Guid>, IAggregateRoot
{
    new Specification<TEntity, TResult> Specification { get; }
    ISpecificationBuilder<TEntity, TResult> Select(Expression<Func<TEntity, TResult>>? selector);
}

public interface ISpecificationBuilder<TEntity> where TEntity : Entity<Guid>, IAggregateRoot
{
    Specification<TEntity> Specification { get; }

    ISpecificationBuilder<TEntity> Where(Expression<Func<TEntity, bool>> criteria);

    ISpecificationBuilder<TEntity> Include(Expression<Func<TEntity, object>> include);

    ISpecificationBuilder<TEntity> Skip(int count);

    ISpecificationBuilder<TEntity> Take(int range);

    ISpecificationBuilder<TEntity> OrderBy(Expression<Func<TEntity, object?>> keySelector);

    ISpecificationBuilder<TEntity> OrderByDescending(Expression<Func<TEntity, object?>> keySelector);
}