using System.Diagnostics;
using Tsutskiridze.TradeBuddy.API.Http;

namespace Tsutskiridze.TradeBuddy.API.Middleware;

internal sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[RequestMetadataHeaderNames.CorrelationId].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Items[RequestMetadataItemKeys.CorrelationId] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[RequestMetadataHeaderNames.CorrelationId] = correlationId;
            return Task.CompletedTask;
        });

        Activity.Current?.SetTag("correlation.id", correlationId);

        await _next(context);
    }
}
