using System.Threading.RateLimiting;

namespace Tsutskiridze.TradeBuddy.Infrastructure.RateLimiting;

public sealed class NoopRateLimiter : RateLimiter
{
    public override TimeSpan? IdleDuration => null;

    public override RateLimiterStatistics? GetStatistics()
    {
        return null;
    }

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        return SuccessfulRateLimitLease.Instance;
    }

    protected override ValueTask<RateLimitLease> AcquireAsyncCore(
        int permitCount,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<RateLimitLease>(
            SuccessfulRateLimitLease.Instance);
    }

    private sealed class SuccessfulRateLimitLease : RateLimitLease
    {
        public static readonly SuccessfulRateLimitLease Instance = new();

        public override bool IsAcquired => true;

        public override IEnumerable<string> MetadataNames =>
            Array.Empty<string>();

        public override bool TryGetMetadata(
            string metadataName,
            out object? metadata)
        {
            metadata = null;
            return false;
        }
    }
}