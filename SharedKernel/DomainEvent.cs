using Mediator;

namespace SharedKernel;

public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccuredAt { get; protected set; } = DateTime.UtcNow;
}