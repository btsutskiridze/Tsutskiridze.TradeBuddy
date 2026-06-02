namespace SharedKernel.Idempotency;

public enum IdempotencyStatus
{
    Processing,
    Completed,
    Failed
}

public class IdempotencyRecord
{
    public long Id { get; private init; }
    public byte[] KeyHash { get; private init; } = null!;
    public byte[] ScopeHash { get; private init; } = null!;
    public string? Key { get; private init; }
    public string? Scope { get; private init; }
    public byte[] RequestHash { get; private init; } = null!;

    public IdempotencyStatus Status { get; private set; }

    public string? ResponseJson { get; private set; }
    public int? StatusCode { get; private set; }
    public string? Error { get; private set; }

    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    private IdempotencyRecord()
    {
    }

    private IdempotencyRecord(
        byte[] keyHash,
        byte[] scopeHash,
        string key,
        string? scope,
        byte[] requestHash,
        IdempotencyStatus status,
        DateTime lockedUntil)
    {
        KeyHash = keyHash;
        ScopeHash = scopeHash;
        Key = key;
        Scope = scope;
        RequestHash = requestHash;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        LockedUntil = lockedUntil;
    }

    public static IdempotencyRecord Started(
        byte[] keyHash,
        byte[] scopeHash,
        string key,
        string? scope,
        byte[] requestHash,
        DateTime lockedUntil)
    {
        ArgumentNullException.ThrowIfNull(keyHash);
        ArgumentNullException.ThrowIfNull(scopeHash);
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(requestHash);

        if (keyHash.Length == 0)
            throw new ArgumentException($"Invalid key provided");

        if (requestHash.Length == 0)
            throw new ArgumentException("Invalid request hash provided", nameof(requestHash));

        return new IdempotencyRecord(
            keyHash,
            scopeHash, 
            key, 
            scope, 
            requestHash, 
            IdempotencyStatus.Processing,
            lockedUntil);
    }

    public void Completed(string responseJson, int? statusCode)
    {
        ArgumentException.ThrowIfNullOrEmpty(responseJson);

        if (statusCode.HasValue && statusCode.Value is < 200 or > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode));
        }

        ResponseJson = responseJson;
        StatusCode = statusCode;
        CompletedAt = DateTime.UtcNow;
        Status = IdempotencyStatus.Completed;
        LockedUntil = null;
    }


    public void Failed(string error, int? statusCode)
    {
        ArgumentException.ThrowIfNullOrEmpty(error);

        if (statusCode.HasValue && statusCode.Value is < 200 or > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode));
        }

        Error = error;
        ResponseJson = "{}";
        StatusCode = statusCode;
        CompletedAt = DateTime.UtcNow;
        LockedUntil = null;
    }

    public void ReProcess(DateTime lockedUntil)
    {
        LockedUntil = lockedUntil;
    }
    
}