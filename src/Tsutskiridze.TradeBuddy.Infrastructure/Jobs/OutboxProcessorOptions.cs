namespace Tsutskiridze.TradeBuddy.Infrastructure.Jobs;

public sealed class OutboxProcessorOptions
{
    public const string SectionName = "Outbox";

    public int PollIntervalSeconds { get; init; } = 5;

    public int BatchSize { get; init; } = 20;

    public int LockDurationSeconds { get; init; } = 120;

    public int MaxRetryDelaySeconds { get; init; } = 300;
    
    public int MaxRetryCount { get; init; } = 3;

    public TimeSpan PollInterval => TimeSpan.FromSeconds(PollIntervalSeconds);

    public TimeSpan LockDuration => TimeSpan.FromSeconds(LockDurationSeconds);
}