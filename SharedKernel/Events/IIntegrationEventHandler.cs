using Mediator;

namespace SharedKernel.Events;

public interface IIntegrationEventHandler<in T> : INotificationHandler<T> where T : IIntegrationEvent
{
}