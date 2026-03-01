using Mediator;

namespace SharedKernel;

public abstract class DomainEvent : INotification
{
    public DateTime OccuredAt { get; protected set; } = DateTime.UtcNow;
}