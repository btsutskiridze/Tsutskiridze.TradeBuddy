using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SharedKernel;
using SharedKernel.Data;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

//todo: refactor and move those into shared kernel later
public class EfRepository<TEntity, TId> : EfReadRepository<TEntity, TId>, IRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    protected override bool UseNoTracking => false;

    public EfRepository(AppDbContext db) : base(db)
    {
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await Set.AddAsync(entity, ct);
        return entity;
    }

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        => Set.AddRangeAsync(entities, ct);

    public void Remove(TEntity entity)
        => Set.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities)
        => Set.RemoveRange(entities);

    public void Update(TEntity entity)
        => Set.Update(entity);

    public async Task<TEntity?> GetByIdForUpdateAsync(
        TId id,
        CancellationToken ct = default)
    {
        if (Db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "A database transaction must be started before using GetByIdForUpdateAsync.");
        }
        await LockByIdAsync(id, ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task LockByIdAsync(TId id, CancellationToken ct)
    {
        var entityType = Db.Model.FindEntityType(typeof(TEntity));

        if (entityType is null)
        {
            throw new InvalidOperationException(
                $"Entity type {typeof(TEntity).FullName} not found in the model.");
        }

        var tableName = entityType.GetTableName();

        if (tableName is null)
        {
            throw new InvalidOperationException(
                $"Table name for entity type {typeof(TEntity).FullName} not found.");
        }

        var schema = entityType.GetSchema();

        var primaryKey = entityType.FindPrimaryKey();

        if (primaryKey is null)
        {
            throw new InvalidOperationException(
                $"Entity type '{typeof(TEntity).Name}' does not have a primary key.");
        }

        if (primaryKey.Properties.Count != 1)
        {
            throw new NotSupportedException(
                $"Entity type '{typeof(TEntity).Name}' must have a single-column primary key.");
        }

        var tableIdentifier = StoreObjectIdentifier.Table(tableName, schema);

        var primaryKeyColumnName = primaryKey.Properties[0]
            .GetColumnName(tableIdentifier);

        if (primaryKeyColumnName is null)
        {
            throw new InvalidOperationException(
                $"Could not resolve primary key column for entity type '{typeof(TEntity).Name}'.");
        }

        var table = schema is null
            ? Quoted(tableName)
            : $"{Quoted(schema)}.{Quoted(tableName)}";

        var primaryKeyColumn = Quoted(primaryKeyColumnName);

        var sql = $$"""
                    select 1
                    from {{table}}
                    where {{primaryKeyColumn}} = {0}
                    for update
                    """;

        var locked = await Db.Database
            .SqlQueryRaw<int>(sql, id)
            .SingleOrDefaultAsync(ct);

        if (locked == 0)
        {
            throw new InvalidOperationException(
                $"{typeof(TEntity).Name} '{id}' does not exist and cannot be locked.");
        }
    }

    private static string Quoted(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"") + "\"";
    }
}