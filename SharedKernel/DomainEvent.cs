using Mediator;

namespace SharedKernel;

public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredAt { get; protected set; } = DateTime.UtcNow;
}