using System.Linq.Expressions;

namespace SharedKernel.Specifications;

public abstract class Specification<TEntity> : ISpecification<TEntity> where TEntity : Entity<Guid>, IAggregateRoot
{
    private readonly List<Expression<Func<TEntity, bool>>> _criterias;
    private readonly List<Expression<Func<TEntity, object>>> _includes;
    protected ISpecificationBuilder<TEntity> Query { get; }

    protected Specification()
    {
        Query = new SpecificationBuilder<TEntity>(this);
    }

    public IReadOnlyList<Expression<Func<TEntity, bool>>> Criterias => _criterias.AsReadOnly();
    public IReadOnlyList<Expression<Func<TEntity, object>>> Includes => _includes.AsReadOnly();
    public Expression<Func<TEntity, object?>>? OrderBy { get; internal set; }
    public Expression<Func<TEntity, object?>>? OrderByDescending { get; internal set; }
    public int? Skip { get; internal set; }
    public int? Take { get; internal set; }

    internal void AddCriteria(Expression<Func<TEntity, bool>> criteria)
    {
        _criterias.Add(criteria);
    }

    internal void AddInclude(Expression<Func<TEntity, object>> include)
    {
        _includes.Add(include);
    }
}