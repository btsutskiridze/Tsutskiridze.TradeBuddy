using System.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private readonly IMediator _mediator;

    public EfUnitOfWork(AppDbContext db, IMediator mediator)
    {
        _db = db;
        _mediator = mediator;
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
        var entitiesWithEvents = _db.ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .Where(e => e.DomainEvents.Any())
            .ToArray();

        var events = entitiesWithEvents
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        int result;
        try
        {
            result = await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(
                "The data was changed by another process.",
                ex);
        }

        /*
         *todo:
         * Introduce an outbox and stop calling SaveChangesAsync
         * from domain-event handlers.
         * Persist state and outbox in one transaction;
         * publish asynchronously.
         *
         */

        foreach (var entity in entitiesWithEvents)
            entity.ClearDomainEvents();

        foreach (var evt in events)
            await _mediator.Publish(evt, ct);

        return result;
    }
}