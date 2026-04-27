using Microsoft.Extensions.DependencyInjection;
using Tsutskiridze.TradeBuddy.Domain.AlertWatching;

namespace Tsutskiridze.TradeBuddy.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<AlertWatchingDomainService>();
        return services;
    }
    
}