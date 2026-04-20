using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Finnhub;

public static class DependencyInjection
{
    public static IServiceCollection AddFinnhubIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FinnhubOptions>(configuration.GetSection(FinnhubOptions.SectionName));
        
        services.AddHttpClient<IFinnhubNewsProvider, FinnhubNewsProvider>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<FinnhubOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        
        return services;
    }
    
}