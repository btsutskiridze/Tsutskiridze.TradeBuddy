namespace Tsutskiridze.TradeBuddy.Application.Abstractions.RateLimiting;


public sealed record RateLimitResult(
    bool IsAllowed,
    TimeSpan? RetryAfter);