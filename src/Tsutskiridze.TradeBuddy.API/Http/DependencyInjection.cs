using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Tsutskiridze.TradeBuddy.API.Middleware;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Http;

namespace Tsutskiridze.TradeBuddy.API.Http;

public static class DependencyInjection
{
    public static IServiceCollection AddRequestMetadata(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestMetadata, RequestMetadataAccessor>();

        services.Configure<ForwardedHeadersOptions>(configuration.GetSection(ForwardedHeadersOptions.SectionName));
        services.AddSingleton<IConfigureOptions<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>, ConfigureForwardedHeadersOptions>();

        return services;
    }

    public static WebApplication UseRequestMetadata(this WebApplication app)
    {
        app.UseForwardedHeaders();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<IdempotencyKeyMiddleware>();
        app.UseMiddleware<ClientMetadataMiddleware>();

        return app;
    }
}
