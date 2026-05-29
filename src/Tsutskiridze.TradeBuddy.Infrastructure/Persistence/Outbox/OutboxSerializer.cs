using System.Text.Json;
using System.Text.Json.Serialization;
using SharedKernel.Events;
using SharedKernel.Outbox;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Outbox;

public class OutboxSerializer : IOutboxSerializer
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = false
    };

    public OutboxMessage Serialize(IDomainEvent domainEvent)
    {
        var serializeType = domainEvent.GetType().AssemblyQualifiedName!;

        var outboxMessage = new OutboxMessage(
            domainEvent.Id,
            domainEvent.EventType,
            1,
            serializeType,
            JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), _options),
            domainEvent.CreateTime,
            DateTime.UtcNow,
            null
        );

        return outboxMessage;
    }
}