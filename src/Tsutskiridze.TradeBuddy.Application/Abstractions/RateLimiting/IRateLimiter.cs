using Tsutskiridze.TradeBuddy.Application.DTOs;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.RateLimiting;

public interface IRateLimiter
{
    Task<RateLimitResult> CheckAsync(string id, string command, string policy, CancellationToken ct = default);
}