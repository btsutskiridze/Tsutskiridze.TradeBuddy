using Tsutskiridze.TradeBuddy.API.Http;

namespace Tsutskiridze.TradeBuddy.API.Middleware;

internal sealed class IdempotencyKeyMiddleware
{
    private readonly RequestDelegate _next;

    public IdempotencyKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var idempotencyKey = context.Request.Headers[RequestMetadataHeaderNames.IdempotencyKey].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            context.Items[RequestMetadataItemKeys.IdempotencyKey] = idempotencyKey;
        }

        await _next(context);
    }
}
