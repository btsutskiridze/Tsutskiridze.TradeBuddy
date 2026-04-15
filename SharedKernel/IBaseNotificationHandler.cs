using Mediator;

namespace SharedKernel;

public interface IBaseNotificationHandler<in T> : INotificationHandler<T> where T : IBaseNotification
{
}