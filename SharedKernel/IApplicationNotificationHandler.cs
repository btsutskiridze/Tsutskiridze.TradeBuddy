using Mediator;

namespace SharedKernel;

public interface IApplicationNotificationHandler<in T> : INotificationHandler<T> where T : IApplicationNotification
{
}