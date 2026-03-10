using Mediator;

namespace SharedKernel;

public abstract record DomainEvent : IDomainEvent
{
    public Guid Id = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}