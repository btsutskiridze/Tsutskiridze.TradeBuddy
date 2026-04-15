using Mediator;
using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications;

public interface INotificationDispatcher
{
    Task DispatchAsync<TNotification>(
        TNotification notification,
        CancellationToken ct = default)
        where TNotification : IBaseNotification;
    
    Task DispatchAsync<TNotification>(
        IReadOnlyCollection<TNotification> notifications,
        CancellationToken ct = default)
        where TNotification : IBaseNotification;
}