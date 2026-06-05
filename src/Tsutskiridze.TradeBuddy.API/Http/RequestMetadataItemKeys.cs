namespace Tsutskiridze.TradeBuddy.API.Http;

internal static class RequestMetadataItemKeys
{
    public const string CorrelationId = "RequestMetadata.CorrelationId";

    public const string IdempotencyKey = "RequestMetadata.IdempotencyKey";

    public const string ClientIp = "RequestMetadata.ClientIp";

    public const string UserAgent = "RequestMetadata.UserAgent";

    public const string Device = "RequestMetadata.Device";
}
