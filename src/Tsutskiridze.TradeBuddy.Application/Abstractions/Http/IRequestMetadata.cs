namespace Tsutskiridze.TradeBuddy.Application.Abstractions.Http;

public interface IRequestMetadata
{
    string CorrelationId { get; }

    string? IdempotencyKey { get; }

    string? ClientIp { get; }

    string? UserAgent { get; }

    string? Device { get; }
}
