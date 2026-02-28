using Mediator;

namespace SharedKernel;

public interface IDomainEvent : INotification
{
    DateTime OccuredAt { get; }
}