using Mediator;

namespace SharedKernel.Events;

public interface IApplicationEventHandler<in T> : IBaseNotificationHandler<T> where T : IApplicationEvent
{
}