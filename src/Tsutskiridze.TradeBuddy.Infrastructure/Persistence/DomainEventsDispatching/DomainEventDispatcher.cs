using SharedKernel.Events;
using SharedKernel.Events.DomainEventsDispatching;
using SharedKernel.Outbox;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.DomainEventsDispatching;

internal sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IOutbox _outbox;
    private readonly IOutboxSerializer _outboxSerializer;

    public DomainEventDispatcher(
        IOutbox outbox,
        IOutboxSerializer outboxSerializer)
    {
        _outbox = outbox;
        _outboxSerializer = outboxSerializer;
    }

    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken ct = default)
    {
        foreach (var domainEvent in domainEvents)
            await PublishAsync(domainEvent, ct);
    }

    private Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        _outbox.Add(_outboxSerializer.Serialize(domainEvent));
        // await _mediator.Publish(domainEvent, ct);
        return Task.CompletedTask;
    }
}