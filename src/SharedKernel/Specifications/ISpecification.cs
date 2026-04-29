using System.Linq.Expressions;

namespace SharedKernel.Specifications;

public interface ISpecification<TEntity, TResult> : ISpecification<TEntity> where TEntity : Entity<Guid>, IAggregateRoot
{
    Expression<Func<TEntity, TResult>>? Selector { get; }
}

public interface ISpecification<TEntity> where TEntity : Entity<Guid>, IAggregateRoot
{
    IReadOnlyList<Expression<Func<TEntity, bool>>> Criterias { get; }

    IReadOnlyList<Expression<Func<TEntity, object>>> Includes { get; }

    Expression<Func<TEntity, object?>>? OrderBy { get; }

    Expression<Func<TEntity, object?>>? OrderByDescending { get; }

    int? Skip { get; }

    int? Take { get; }
}