namespace SharedKernel.Outbox;

public enum OutboxMessageStatus
{
    Pending = 1,
    Processing = 2,
    Processed = 3,
    Dead = 4
}

public sealed class OutboxMessage
{
    private const int MaxErrorLength = 4000;

    public Guid Id { get; private init; }
    public string EventType { get; private init; }
    public int EventVersion { get; private init; }
    public string SerializeType { get; private init; }
    public string Payload { get; private init; }
    public string? Headers { get; private init; }
    public OutboxMessageStatus Status { get; private set; }
    public DateTime OccurredAt { get; private init; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? DeadAt { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public string? LastError { get; private set; }
    public Guid? LockId { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    public bool IsPending => Status == OutboxMessageStatus.Pending;
    public bool IsProcessing => Status == OutboxMessageStatus.Processing;
    public bool IsProcessed => Status == OutboxMessageStatus.Processed;
    public bool IsDead => Status == OutboxMessageStatus.Dead;

    public OutboxMessage(
        Guid id,
        string eventType,
        int eventVersion,
        string serializeType,
        string payload,
        DateTime occurredAt,
        DateTime createdAt,
        string? headers = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Outbox message id is required.", nameof(id));

        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type is required.", nameof(eventType));

        if (eventVersion <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(eventVersion),
                "Event version must be greater than zero.");

        if (string.IsNullOrWhiteSpace(serializeType))
            throw new ArgumentException("Serialize type is required.", nameof(serializeType));

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload is required.", nameof(payload));

        Id = id;
        EventType = eventType.Trim();
        EventVersion = eventVersion;
        SerializeType = serializeType.Trim();
        Payload = payload;
        Headers = headers;
        OccurredAt = occurredAt;
        CreatedAt = createdAt;

        Status = OutboxMessageStatus.Pending;
    }

    private OutboxMessage()
    {
        EventType = null!;
        SerializeType = null!;
        Payload = null!;
    }

    public void MarkProcessing(Guid lockId, DateTime lockedUntil)
    {
        if (lockId == Guid.Empty)
            throw new ArgumentException("Lock id is required.", nameof(lockId));

        if (Status != OutboxMessageStatus.Pending)
            throw new InvalidOperationException("Only pending outbox messages can be marked as processing.");

        Status = OutboxMessageStatus.Processing;
        LockId = lockId;
        LockedUntil = lockedUntil;
    }

    public void MarkProcessed(DateTime processedAt)
    {
        if (Status == OutboxMessageStatus.Processed)
            return;

        if (Status == OutboxMessageStatus.Dead)
            throw new InvalidOperationException("Dead outbox messages cannot be marked as processed.");

        Status = OutboxMessageStatus.Processed;
        ProcessedAt = processedAt;

        ClearRetry();
        ClearLock();

        LastError = null;
    }

    public void MarkFailed(string error, DateTime nextRetryAt)
    {
        if (Status == OutboxMessageStatus.Processed)
            throw new InvalidOperationException("Processed outbox messages cannot be marked as failed.");

        if (Status == OutboxMessageStatus.Dead)
            throw new InvalidOperationException("Dead outbox messages cannot be marked as failed.");

        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error is required.", nameof(error));

        RetryCount++;
        LastError = Truncate(error, MaxErrorLength);
        NextRetryAt = nextRetryAt;

        Status = OutboxMessageStatus.Pending;

        ClearLock();
    }

    public void MarkDead(string error, DateTime deadAt)
    {
        if (Status == OutboxMessageStatus.Processed)
            throw new InvalidOperationException("Processed outbox messages cannot be marked as dead.");

        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error is required.", nameof(error));

        RetryCount++;
        LastError = Truncate(error, MaxErrorLength);
        DeadAt = deadAt;

        Status = OutboxMessageStatus.Dead;

        ClearRetry();
        ClearLock();
    }

    public bool IsReadyToProcess(DateTime utcNow)
    {
        return Status == OutboxMessageStatus.Pending &&
               (NextRetryAt is null || NextRetryAt <= utcNow);
    }

    public bool IsLockExpired(DateTime utcNow)
    {
        return Status == OutboxMessageStatus.Processing &&
               LockedUntil is not null &&
               LockedUntil <= utcNow;
    }

    private void ClearRetry()
    {
        NextRetryAt = null;
    }

    private void ClearLock()
    {
        LockId = null;
        LockedUntil = null;
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : value[..maxLength];
    }
}