using Microsoft.Extensions.DependencyInjection;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.GoogleNews;

public static class DependencyInjection
{
    public static IServiceCollection AddGoogleNewsIntegration(this IServiceCollection services)
    {
        services.AddHttpClient<IGoogleNewsProvider, GoogleNewsProvider>();

        return services;
    }
}
