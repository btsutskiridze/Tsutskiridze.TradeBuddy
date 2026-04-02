using Mediator;
using SharedKernel;
using SharedKernel.Events;

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

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var entitiesWithEvents = _db.ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .Where(e => e.DomainEvents.Any())
            .ToArray();

        var events = new List<IDomainEvent>();
        foreach (var entity in entitiesWithEvents)
        {
            events.AddRange(entity.DomainEvents);
            entity.ClearDomainEvents();
        }

        var result = await _db.SaveChangesAsync(ct);

        foreach (var evt in events)
            await _mediator.Publish(evt, ct);

        return result;
    }
}
