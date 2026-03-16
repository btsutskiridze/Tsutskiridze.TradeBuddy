using Mediator;

namespace SharedKernel.Events;

public interface IDomainEventHandler<in T> : INotificationHandler<T> where T : IDomainEvent
{
}