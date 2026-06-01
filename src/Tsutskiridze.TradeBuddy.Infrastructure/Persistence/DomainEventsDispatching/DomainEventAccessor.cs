using SharedKernel;
using SharedKernel.Events;
using SharedKernel.Events.DomainEventsDispatching;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.DomainEventsDispatching;

internal sealed class DomainEventAccessor : IDomainEventAccessor
{
    private readonly AppDbContext _db;

    public DomainEventAccessor(AppDbContext db)
    {
        _db = db;
    }

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
    {
        return GetEventSources()
            .SelectMany(entity => entity.DomainEvents)
            .ToArray();
    }

    public void ClearDomainEvents()
    {
        foreach (var source in GetEventSources())
        {
            source.ClearDomainEvents();
        }
    }

    private IReadOnlyCollection<IHasDomainEvents> GetEventSources()
    {
        return _db.ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .Where(e => e.DomainEvents.Count > 0)
            .ToArray();
    }
}
