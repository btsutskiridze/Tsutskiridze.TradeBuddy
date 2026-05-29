using System.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Data;
using SharedKernel.Events.DomainEventsDispatching;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Exceptions;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence;

internal class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private readonly IDomainEventAccessor _domainEventAccessor;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IDbExceptionClassifier _dbExceptionClassifier;

    public EfUnitOfWork(
        AppDbContext db,
        IDomainEventAccessor domainEventAccessor,
        IDomainEventDispatcher domainEventDispatcher,
        IDbExceptionClassifier dbExceptionClassifier)
    {
        _db = db;
        _domainEventAccessor = domainEventAccessor;
        _domainEventDispatcher = domainEventDispatcher;
        _dbExceptionClassifier = dbExceptionClassifier;
    }

    public async Task<IAppDbTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken ct = default)
    {
        var tx = await _db.Database.BeginTransactionAsync(isolationLevel, ct);

        return new EfAppDbTransaction(tx);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var domainEvents = _domainEventAccessor.GetDomainEvents();

        try
        {
            _domainEventAccessor.ClearDomainEvents();
            await _domainEventDispatcher.DispatchAsync(domainEvents, ct);

            var result = await _db.SaveChangesAsync(ct);
            return result;
        }
        catch (DbUpdateException ex) when (_dbExceptionClassifier.Translate(ex) is { } translated)
        {
            throw translated;
        }
    }
}
