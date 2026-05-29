using SharedKernel.Events;

namespace SharedKernel.Outbox;

public interface IOutboxSerializer
{
    OutboxMessage Serialize(IDomainEvent @event);
}