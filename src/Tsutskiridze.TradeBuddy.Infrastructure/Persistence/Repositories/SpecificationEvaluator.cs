using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

public static class SpecificationEvaluator<TEntity, TId> 
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    public static IQueryable<TEntity> ShapeQuery(ISpecification<TEntity, TId> specification, IQueryable<TEntity> query)
    {
        query = ShapeBaseQuery(specification, query);

        return query;
    }

    public static IQueryable<TResult> ShapeQuery<TResult>(ISpecification<TEntity, TId, TResult> specification,
        IQueryable<TEntity> query)
    {
        query = ShapeBaseQuery(specification, query);

        if (specification.Selector is null)
            throw new InvalidOperationException("Projection specification requires Selector.");

        return query.Select(specification.Selector);
    }

    private static IQueryable<TEntity> ShapeBaseQuery(ISpecification<TEntity, TId> specification, IQueryable<TEntity> query)
    {
        query = specification.Criterias.Aggregate(query, (current, criteria) => current.Where(criteria));

        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        foreach (var include in specification.Includes)
        {
            query = query.Include(include);
        }

        if (specification.Skip.HasValue)
        {
            query = query.Skip(specification.Skip.Value);
        }

        if (specification.Take.HasValue)
        {
            query = query.Take(specification.Take.Value);
        }

        return query;
    }
}