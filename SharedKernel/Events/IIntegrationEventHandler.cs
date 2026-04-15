using Mediator;

namespace SharedKernel.Events;

public interface IIntegrationEventHandler<in T> : IBaseNotificationHandler<T> where T : IIntegrationEvent
{
}