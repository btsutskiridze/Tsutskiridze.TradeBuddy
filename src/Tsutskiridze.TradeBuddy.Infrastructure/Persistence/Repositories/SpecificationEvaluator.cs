using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

public static class SpecificationEvaluator<TEntity> where TEntity : Entity<Guid>, IAggregateRoot
{
    public static IQueryable<TEntity> ShapeQuery(ISpecification<TEntity> specification, IQueryable<TEntity> query)
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
            query = query.Skip(specification.Take.Value);
        }

        return query;
    }
}