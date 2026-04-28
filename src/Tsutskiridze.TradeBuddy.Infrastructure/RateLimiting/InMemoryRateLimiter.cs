using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using Tsutskiridze.TradeBuddy.Application.Abstractions.RateLimiting;
using Tsutskiridze.TradeBuddy.Application.DTOs;

namespace Tsutskiridze.TradeBuddy.Infrastructure.RateLimiting;

public sealed class InMemoryRateLimiter : IRateLimiter, IAsyncDisposable
{
    private readonly RateLimitOptions _options;

    private readonly PartitionedRateLimiter<CommandRateLimitRequest> _limiter;

    public InMemoryRateLimiter(IOptions<RateLimitOptions> options)
    {
        _options = options.Value;

        _limiter = PartitionedRateLimiter.Create<CommandRateLimitRequest, string>(
            request =>
            {
                if (!_options.Enabled)
                {
                    return RateLimitPartition.GetNoLimiter("disabled");
                }

                if (!_options.Policies.TryGetValue(request.PolicyName, out var policy))
                {
                    throw new InvalidOperationException(
                        $"Rate limit policy '{request.PolicyName}' is not configured.");
                }

                if (!policy.Enabled)
                {
                    return RateLimitPartition.GetNoLimiter("disabled");
                }

                var partitionKey = BuildPartitionKey(request, policy);

                return RateLimitPartition.Get(
                    partitionKey,
                    _ => BuildRateLimiter(policy));
            });
    }

    public async Task<RateLimitResult> CheckAsync(
        string userId,
        string commandName,
        string policyName,
        CancellationToken ct)
    {
        var policy = _options.Policies[policyName];

        var request = new CommandRateLimitRequest(
            Id: userId,
            CommandName: commandName,
            PolicyName: policyName);

        using var lease = await _limiter.AcquireAsync(
            request,
            permitCount: policy.PermitCost,
            cancellationToken: ct);

        if (lease.IsAcquired)
        {
            return new RateLimitResult(true, null);
        }

        lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter);

        return new RateLimitResult(false, retryAfter);
    }

    private static string BuildPartitionKey(
        CommandRateLimitRequest request,
        CommandRateLimitPolicyOptions policy)
    {
        return policy.PartitionScope switch
        {
            RateLimitPartitionScope.User =>
                $"user:{request.Id}",

            RateLimitPartitionScope.UserAndCommand =>
                $"user:{request.Id}:command:{request.CommandName}",

            RateLimitPartitionScope.GlobalCommand =>
                $"global:command:{request.CommandName}",

            _ => throw new NotSupportedException(
                $"Partition scope '{policy.PartitionScope}' is not supported.")
        };
    }

    private static RateLimiter BuildRateLimiter(
        CommandRateLimitPolicyOptions policy)
    {
        var limiters = policy.Rules
            .Where(x => x.Enabled)
            .Select(BuildRuleLimiter)
            .ToArray();

        if (limiters.Length == 0)
        {
            return new NoopRateLimiter();
        }

        if (limiters.Length == 1)
        {
            return limiters[0];
        }

        return RateLimiter.CreateChained(limiters);
    }

    private static RateLimiter BuildRuleLimiter(RateLimitRuleOptions rule)
    {
        return rule.Algorithm switch
        {
            RateLimitAlgorithm.FixedWindow => new FixedWindowRateLimiter(
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rule.PermitLimit,
                    Window = rule.Window ?? throw Missing(nameof(rule.Window)),
                    QueueLimit = rule.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                }),

            RateLimitAlgorithm.SlidingWindow => new SlidingWindowRateLimiter(
                new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = rule.PermitLimit,
                    Window = rule.Window ?? throw Missing(nameof(rule.Window)),
                    SegmentsPerWindow = rule.SegmentsPerWindow
                        ?? throw Missing(nameof(rule.SegmentsPerWindow)),
                    QueueLimit = rule.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                }),

            RateLimitAlgorithm.TokenBucket => new TokenBucketRateLimiter(
                new TokenBucketRateLimiterOptions
                {
                    TokenLimit = rule.PermitLimit,
                    TokensPerPeriod = rule.TokensPerPeriod
                        ?? throw Missing(nameof(rule.TokensPerPeriod)),
                    ReplenishmentPeriod = rule.ReplenishmentPeriod
                        ?? throw Missing(nameof(rule.ReplenishmentPeriod)),
                    QueueLimit = rule.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                }),

            RateLimitAlgorithm.Concurrency => new ConcurrencyLimiter(
                new ConcurrencyLimiterOptions
                {
                    PermitLimit = rule.PermitLimit,
                    QueueLimit = rule.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                }),

            _ => throw new NotSupportedException(
                $"Rate limit algorithm '{rule.Algorithm}' is not supported.")
        };

        static InvalidOperationException Missing(string propertyName)
        {
            return new InvalidOperationException(
                $"Missing required rate limit property: {propertyName}.");
        }
    }

    public ValueTask DisposeAsync()
    {
        return _limiter.DisposeAsync();
    }

    private sealed record CommandRateLimitRequest(
        string Id,
        string CommandName,
        string PolicyName);
}