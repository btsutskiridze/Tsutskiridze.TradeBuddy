using Microsoft.Extensions.DependencyInjection;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.EventHandlers;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis.Services;

namespace Tsutskiridze.TradeBuddy.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<StockAnalysisPromptBuilder>();
        services.AddTransient<StockAnalysisGenerator>();
        services.AddSingleton<PricingUpdatedIntegrationEventHandler>();
        
        return services;
    }
}
