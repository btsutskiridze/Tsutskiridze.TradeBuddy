using Microsoft.Extensions.DependencyInjection;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.GoogleNews;

public static class DependencyInjection
{
    public static IServiceCollection AddGoogleNewsIntegration(this IServiceCollection services)
    {
        services.AddHttpClient<IGoogleNewsProvider, GoogleNewsProvider>();        
     
        return services;
    }
    
}