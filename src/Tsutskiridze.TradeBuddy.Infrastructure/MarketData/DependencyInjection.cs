using Microsoft.Extensions.DependencyInjection;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData;

public static class DependencyInjection
{
    public static IServiceCollection AddMarketDataServices(this IServiceCollection services)
    {
        services.AddScoped<IMarketDataProvider, MarketDataProvider>();
        services.AddScoped<IMarketDataListener, MarketDataListener>();
        
        return services;
    }
}