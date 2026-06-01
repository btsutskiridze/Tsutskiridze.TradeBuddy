using SharedKernel.Events;

namespace SharedKernel;

public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public abstract string EventType { get; }
    public DateTime CreateTime { get; init; } = DateTime.UtcNow;
}