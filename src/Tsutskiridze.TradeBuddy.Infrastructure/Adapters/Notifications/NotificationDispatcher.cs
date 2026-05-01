using Mediator;
using SharedKernel;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Adapters.Notifications;

public sealed class NotificationDispatcher : INotificationDispatcher
{
    private readonly IMediator _mediator;

    public NotificationDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchAsync<TNotification>(TNotification notification, CancellationToken ct = default)
        where TNotification : IBaseNotification
    {
        await _mediator.Publish(notification, ct);
    }

    public async Task DispatchAsync<TNotification>( IReadOnlyCollection<TNotification> notifications, CancellationToken ct = default)
        where TNotification : IBaseNotification
    {
        foreach (var notification in notifications)
        {
            ct.ThrowIfCancellationRequested();
            await DispatchAsync(notification, ct);
        }
    }
}