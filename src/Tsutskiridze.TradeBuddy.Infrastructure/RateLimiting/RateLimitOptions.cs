namespace Tsutskiridze.TradeBuddy.Infrastructure.RateLimiting;

public class RateLimitOptions
{
    public const string SectionName = "RateLimits";
    public bool Enabled { get; init; } = true;
    public Dictionary<string, CommandRateLimitPolicyOptions> Policies { get; init; } = new();
}

public sealed class CommandRateLimitPolicyOptions
{
    public bool Enabled { get; init; } = true;

    public RateLimitPartitionScope PartitionScope { get; init; } =
        RateLimitPartitionScope.User;

    public int PermitCost { get; init; } = 1;

    public List<RateLimitRuleOptions> Rules { get; init; } = [];
}

public sealed class RateLimitRuleOptions
{
    public bool Enabled { get; init; } = true;

    public RateLimitAlgorithm Algorithm { get; init; }

    public int PermitLimit { get; init; }

    public TimeSpan? Window { get; init; }

    public int? SegmentsPerWindow { get; init; }

    public TimeSpan? ReplenishmentPeriod { get; init; }

    public int? TokensPerPeriod { get; init; }

    public int QueueLimit { get; init; } = 0;
}

public enum RateLimitAlgorithm
{
    FixedWindow = 1,
    SlidingWindow = 2,
    TokenBucket = 3,
    Concurrency = 4
}

public enum RateLimitPartitionScope
{
    User = 1,
    UserAndCommand = 2,
    GlobalCommand = 3,
}