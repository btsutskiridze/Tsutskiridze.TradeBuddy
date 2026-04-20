using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep;

public static class DependencyInjection
{
    public static IServiceCollection AddFinancialModelingPrepIntegration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FinancialModelingPrepOptions>(configuration.GetSection(FinancialModelingPrepOptions.SectionName));
        
        services.AddHttpClient<IFinancialModelingPrepQuoteProvider, FinancialModelingPrepClient>((serviceProvider,
            client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<FinancialModelingPrepOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        
        return services;
    }
}