using Microsoft.Extensions.DependencyInjection;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Http;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.GoogleNews;

public static class DependencyInjection
{
    public static IServiceCollection AddGoogleNewsIntegration(this IServiceCollection services)
    {
        services.AddHttpClient<IGoogleNewsProvider, GoogleNewsProvider>()
            .AddResiliencePipeline(GoogleNewsOptions.ResiliencePipelineName);

        return services;
    }
}
