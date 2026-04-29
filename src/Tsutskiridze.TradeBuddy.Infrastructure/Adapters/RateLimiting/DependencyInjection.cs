using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tsutskiridze.TradeBuddy.Application.Abstractions.RateLimiting;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Adapters.RateLimiting;

public static class DependencyInjection
{

    public static IServiceCollection AddRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitOptions>(configuration.GetSection(RateLimitOptions.SectionName));
        services.AddSingleton<IRateLimiter, InMemoryRateLimiter>();
        
        return services;
    }
    
}