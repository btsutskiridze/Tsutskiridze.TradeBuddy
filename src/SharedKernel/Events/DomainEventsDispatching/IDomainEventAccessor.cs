namespace SharedKernel.Events.DomainEventsDispatching;

public interface IDomainEventAccessor
{
    IReadOnlyCollection<IDomainEvent> GetDomainEvents();
    void ClearDomainEvents();
}
