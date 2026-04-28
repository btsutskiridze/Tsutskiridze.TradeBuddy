namespace Tsutskiridze.TradeBuddy.Application.DTOs;


public sealed record RateLimitResult(
    bool IsAllowed,
    TimeSpan? RetryAfter);