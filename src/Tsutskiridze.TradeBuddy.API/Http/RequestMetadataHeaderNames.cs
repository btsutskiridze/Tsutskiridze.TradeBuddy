namespace Tsutskiridze.TradeBuddy.API.Http;

internal static class RequestMetadataHeaderNames
{
    public const string CorrelationId = "X-Correlation-Id";

    public const string IdempotencyKey = "Idempotency-Key";

    public const string Device = "X-Device-Id";

    public const string ForwardedFor = "X-Forwarded-For";
}
