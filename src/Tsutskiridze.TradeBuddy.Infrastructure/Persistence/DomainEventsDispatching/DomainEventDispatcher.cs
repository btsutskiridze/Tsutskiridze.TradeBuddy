using Mediator;
using SharedKernel.Events;
using SharedKernel.Events.DomainEventsDispatching;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.DomainEventsDispatching;

internal sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken ct = default)
    {
        foreach (var domainEvent in domainEvents)
            await PublishAsync(domainEvent, ct);
    }

    private async Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        /*
         * todo: enqueue to outbox in the same transaction as SaveChanges,
         * then publish asynchronously from a background processor.
         */
        await _mediator.Publish(domainEvent, ct);
    }
}
