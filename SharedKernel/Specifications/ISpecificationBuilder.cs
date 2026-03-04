using System.Linq.Expressions;

namespace SharedKernel.Specifications;

public interface ISpecificationBuilder<TEntity> where TEntity : Entity<Guid>, IAggregateRoot
{
    Specification<TEntity> Specification { get; }

    SpecificationBuilder<TEntity> Where(Expression<Func<TEntity, bool>> criteria);

    SpecificationBuilder<TEntity> Include(Expression<Func<TEntity, object>> include);

    SpecificationBuilder<TEntity> Skip(int count);

    SpecificationBuilder<TEntity> Take(int range);

    SpecificationBuilder<TEntity> OrderBy(Expression<Func<TEntity, object?>> keySelector);

    SpecificationBuilder<TEntity> OrderByDescending(Expression<Func<TEntity, object?>> keySelector);
}