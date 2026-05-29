namespace SharedKernel.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private init; }
    public string EventType { get; private init; }
    public int EventVersion { get; private init; }
    public string SerializeType { get; private init; }
    public string Payload { get; private init; }
    public string? Headers { get; private init; }
    public DateTime OccurTime { get; private init; }
    public DateTime CreateTime { get; private init; }
    public DateTime? ProcessDate { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? NextRetryTime { get; private set; }
    public string? LastError { get; private set; }

    public bool IsProcessed => ProcessDate.HasValue;
    public bool IsReadyToProcess => !IsProcessed && (NextRetryTime is null || NextRetryTime <= DateTime.UtcNow);

    public OutboxMessage(
        Guid id,
        string eventType,
        int eventVersion,
        string serializeType,
        string payload,
        DateTime occurTime,
        DateTime createTime,
        string? headers = null)
    {
        if (id == Guid.Empty) throw new ArgumentException("Outbox message id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type is required.", nameof(eventType));
        if (eventVersion <= 0)
            throw new ArgumentOutOfRangeException(nameof(eventVersion), "Event version must be greater than zero.");
        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload is required.", nameof(payload));

        if (string.IsNullOrWhiteSpace(serializeType))
            throw new ArgumentException("Serialize type is required.", nameof(serializeType));

        Id = id;
        EventType = eventType;
        EventVersion = eventVersion;
        SerializeType = serializeType;
        Payload = payload;
        Headers = headers;
        OccurTime = occurTime;
        CreateTime = createTime;
    }

    private OutboxMessage()
    {
        EventType = null!;
        Payload = null!;
    }

    public void MarkProcessed(DateTime processDate)
    {
        ProcessDate = processDate;
        NextRetryTime = null;
        LastError = null;
    }

    public void MarkFailed(string error, DateTime nextRetryTime)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error is required.", nameof(error));

        RetryCount++;
        LastError = error;
        NextRetryTime = nextRetryTime;
    }
}
