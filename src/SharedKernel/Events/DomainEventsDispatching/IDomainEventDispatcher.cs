namespace SharedKernel.Events.DomainEventsDispatching;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken ct = default);
}
