namespace Tsutskiridze.TradeBuddy.Application.Abstractions.RateLimiting;

public interface IRateLimitedMessage
{
    string Id { get; }
    string Command => GetType().Name;
    string Policy { get; }
}