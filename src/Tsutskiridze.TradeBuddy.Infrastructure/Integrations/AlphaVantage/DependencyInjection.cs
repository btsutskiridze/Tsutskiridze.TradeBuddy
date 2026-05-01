using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.AlphaVantage;

public static class DependencyInjection
{
    public static IServiceCollection AddAlphaVantageIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AlphaVantageOptions>(configuration.GetSection(AlphaVantageOptions.SectionName));
        
        services.AddHttpClient<IAlphaVantageMarketDataProvider, AlphaVantageClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<AlphaVantageOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        
        return services;
    }
}