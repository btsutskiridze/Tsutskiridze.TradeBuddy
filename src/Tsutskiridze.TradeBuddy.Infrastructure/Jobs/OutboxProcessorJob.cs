using System.Text.Json;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Events;
using SharedKernel.Outbox;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence;
using Tsutskiridze.TradeBuddy.Infrastructure.Serialization;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Jobs;

public sealed class OutboxProcessorJob : BackgroundService
{
    private const int MaxErrorLength = 4000;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorJob> _logger;
    private readonly OutboxProcessorOptions _options;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public OutboxProcessorJob(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessorJob> logger,
        IOptions<OutboxProcessorOptions> options,
        InfraJsonSerializerOptions jsonSerializerOptions)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
        _jsonSerializerOptions = jsonSerializerOptions.Options;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        ValidateOptions();

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var processedCount = await ProcessBatchAsync(ct);

                if (processedCount == 0)
                    await Task.Delay(_options.PollInterval, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing outbox messages");

                await Task.Delay(_options.PollInterval, ct);
            }
        }
    }

    private async Task<int> ProcessBatchAsync(CancellationToken ct)
    {
        await RecoverExpiredLocksAsync(ct);

        var messages = await ClaimMessagesAsync(ct);

        foreach (var message in messages)
        {
            await ProcessMessageAsync(message, ct);
        }

        return messages.Count;
    }

    private async Task<IReadOnlyList<ClaimedOutboxMessage>> ClaimMessagesAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var lockId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var lockedUntil = now.Add(_options.LockDuration);

        var pendingStatus = nameof(OutboxMessageStatus.Pending);
        var processingStatus = nameof(OutboxMessageStatus.Processing);

        return await db.Database
            .SqlQuery<ClaimedOutboxMessage>($"""
                WITH messages_to_claim AS (
                    SELECT id
                    FROM outbox_messages
                    WHERE status = {pendingStatus}
                      AND retry_count < {_options.MaxRetryCount}
                      AND (next_retry_at IS NULL OR next_retry_at <= {now})
                    ORDER BY occurred_at
                    LIMIT {_options.BatchSize}
                    FOR UPDATE SKIP LOCKED
                )
                UPDATE outbox_messages AS message
                SET status = {processingStatus},
                    lock_id = {lockId},
                    locked_until = {lockedUntil}
                FROM messages_to_claim
                WHERE message.id = messages_to_claim.id
                RETURNING
                    message.id,
                    message.event_type,
                    message.event_version,
                    message.serialize_type,
                    message.payload,
                    message.headers,
                    message.retry_count,
                    message.lock_id,
                    message.occurred_at;
                """)
            .ToListAsync(ct);
    }

    private async Task RecoverExpiredLocksAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;
        var pendingStatus = nameof(OutboxMessageStatus.Pending);
        var processingStatus = nameof(OutboxMessageStatus.Processing);

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE outbox_messages
            SET status = {pendingStatus},
                lock_id = NULL,
                locked_until = NULL,
                next_retry_at = {now}
            WHERE status = {processingStatus}
              AND locked_until IS NOT NULL
              AND locked_until <= {now};
            """, ct);
    }

    private async Task ProcessMessageAsync(
        ClaimedOutboxMessage message,
        CancellationToken ct)
    {
        try
        {
            await using (var scope = _scopeFactory.CreateAsyncScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var domainEvent = DeserializeDomainEvent(message);

                await mediator.Publish(domainEvent, ct);
            }

            await MarkProcessedAsync(message.Id, message.LockId, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await MarkFailedOrDeadAsync(message.Id, message.LockId, ex, ct);
        }
    }

    private IDomainEvent DeserializeDomainEvent(ClaimedOutboxMessage message)
    {
        var eventType = Type.GetType(message.SerializeType, throwOnError: false)
            ?? throw new InvalidOperationException(
                $"Event type '{message.SerializeType}' was not found.");

        var domainEvent = JsonSerializer.Deserialize(
            message.Payload,
            eventType,
            _jsonSerializerOptions) as IDomainEvent;

        return domainEvent
            ?? throw new InvalidOperationException(
                $"Payload could not be deserialized as {nameof(IDomainEvent)}.");
    }

    private async Task MarkProcessedAsync(
        Guid messageId,
        Guid lockId,
        CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var message = await db.OutboxMessages
            .Where(x =>
                x.Id == messageId &&
                x.LockId == lockId &&
                x.Status == OutboxMessageStatus.Processing)
            .FirstOrDefaultAsync(ct);

        if (message is null)
        {
            _logger.LogWarning(
                "Outbox message {MessageId} was not marked as processed because lock ownership was lost",
                messageId);

            return;
        }

        message.MarkProcessed(DateTime.UtcNow);

        await db.SaveChangesAsync(ct);
    }

    private async Task MarkFailedOrDeadAsync(
        Guid messageId,
        Guid lockId,
        Exception ex,
        CancellationToken ct)
    {
        _logger.LogError(ex, "Failed to process outbox message {MessageId}", messageId);

        await using var scope = _scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var message = await db.OutboxMessages
            .Where(x =>
                x.Id == messageId &&
                x.LockId == lockId &&
                x.Status == OutboxMessageStatus.Processing)
            .FirstOrDefaultAsync(ct);

        if (message is null)
        {
            _logger.LogWarning(
                "Outbox message {MessageId} was not marked as failed because lock ownership was lost",
                messageId);

            return;
        }

        var error = ex.ToString();
        var nextRetryCount = message.RetryCount + 1;

        if (nextRetryCount >= _options.MaxRetryCount)
        {
            message.MarkDead(error, DateTime.UtcNow);
        }
        else
        {
            message.MarkFailed(error, GetNextRetryTime(nextRetryCount));
        }

        await db.SaveChangesAsync(ct);
    }

    private DateTime GetNextRetryTime(int retryCount)
    {
        var delaySeconds = Math.Min(
            _options.MaxRetryDelaySeconds,
            Math.Pow(2, retryCount - 1) * 5);

        return DateTime.UtcNow.AddSeconds(delaySeconds);
    }

    private void ValidateOptions()
    {
        if (_options.BatchSize <= 0)
            throw new InvalidOperationException("Outbox batch size must be greater than zero.");

        if (_options.MaxRetryCount <= 0)
            throw new InvalidOperationException("Outbox max retry count must be greater than zero.");

        if (_options.MaxRetryDelaySeconds <= 0)
            throw new InvalidOperationException("Outbox max retry delay must be greater than zero.");

        if (_options.LockDuration <= TimeSpan.Zero)
            throw new InvalidOperationException("Outbox lock duration must be greater than zero.");
    }

    private sealed class ClaimedOutboxMessage
    {
        public Guid Id { get; init; }

        public string EventType { get; init; } = null!;

        public int EventVersion { get; init; }

        public string SerializeType { get; init; } = null!;

        public string Payload { get; init; } = null!;

        public string? Headers { get; init; }

        public int RetryCount { get; init; }

        public Guid LockId { get; init; }

        public DateTime OccurredAt { get; init; }
    }
}