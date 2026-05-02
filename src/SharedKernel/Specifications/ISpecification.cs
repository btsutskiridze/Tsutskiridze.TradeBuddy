using System.Linq.Expressions;

namespace SharedKernel.Specifications;

public interface ISpecification<TEntity, TId, TResult> : ISpecification<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    Expression<Func<TEntity, TResult>>? Selector { get; }
}

public interface ISpecification<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    IReadOnlyList<Expression<Func<TEntity, bool>>> Criterias { get; }

    IReadOnlyList<Expression<Func<TEntity, object>>> Includes { get; }

    Expression<Func<TEntity, object?>>? OrderBy { get; }

    Expression<Func<TEntity, object?>>? OrderByDescending { get; }

    int? Skip { get; }

    int? Take { get; }
}