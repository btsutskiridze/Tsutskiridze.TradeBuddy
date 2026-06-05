using Tsutskiridze.TradeBuddy.Application.Abstractions.Http;

namespace Tsutskiridze.TradeBuddy.API.Http;

internal sealed class RequestMetadataAccessor : IRequestMetadata
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestMetadataAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string CorrelationId => GetRequired(RequestMetadataItemKeys.CorrelationId);

    public string? IdempotencyKey => GetOptional(RequestMetadataItemKeys.IdempotencyKey);

    public string? ClientIp => GetOptional(RequestMetadataItemKeys.ClientIp);

    public string? UserAgent => GetOptional(RequestMetadataItemKeys.UserAgent);

    public string? Device => GetOptional(RequestMetadataItemKeys.Device);

    private string GetRequired(string key) =>
        _httpContextAccessor.HttpContext?.Items[key] as string ?? string.Empty;

    private string? GetOptional(string key) =>
        _httpContextAccessor.HttpContext?.Items[key] as string;
}
