using System.Net;
using Tsutskiridze.TradeBuddy.API.Http;

namespace Tsutskiridze.TradeBuddy.API.Middleware;

internal sealed class ClientMetadataMiddleware
{
    private readonly RequestDelegate _next;

    public ClientMetadataMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = ResolveClientIp(context);
        if (!string.IsNullOrWhiteSpace(clientIp))
        {
            context.Items[RequestMetadataItemKeys.ClientIp] = clientIp;
        }

        var userAgent = context.Request.Headers.UserAgent.ToString();
        if (!string.IsNullOrWhiteSpace(userAgent))
        {
            context.Items[RequestMetadataItemKeys.UserAgent] = userAgent;
        }

        var device = context.Request.Headers[RequestMetadataHeaderNames.Device].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(device))
        {
            context.Items[RequestMetadataItemKeys.Device] = device;
        }

        await _next(context);
    }

    private static string? ResolveClientIp(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress;

        return remoteIp is null ? null : NormalizeIp(remoteIp);
    }

    private static string NormalizeIp(IPAddress ipAddress)
    {
        if (ipAddress.IsIPv4MappedToIPv6)
        {
            return ipAddress.MapToIPv4().ToString();
        }

        return ipAddress.ToString();
    }
}
