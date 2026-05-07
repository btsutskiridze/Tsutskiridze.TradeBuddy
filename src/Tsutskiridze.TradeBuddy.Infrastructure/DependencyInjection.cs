using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications;
using Tsutskiridze.TradeBuddy.Infrastructure.Adapters.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.Adapters.News;
using Tsutskiridze.TradeBuddy.Infrastructure.Adapters.Notifications;
using Tsutskiridze.TradeBuddy.Infrastructure.Adapters.RateLimiting;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.AlphaVantage;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Finnhub;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Gemini;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.GoogleNews;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.OpenAI;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo;
using Tsutskiridze.TradeBuddy.Infrastructure.Jobs;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence;

namespace Tsutskiridze.TradeBuddy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddPersistence(configuration)
            .AddNotifications()
            .AddRateLimiter(configuration)
            .AddIntegrations(configuration)
            .AddMarketDataServices()
            .AddNewsServices()
            .AddJobs();

        return services;
    }
    
    private static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        return services;
    }

    private static IServiceCollection AddIntegrations(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddTelegramIntegration(configuration)
            .AddAlphaVantageIntegration(configuration)
            .AddFinancialModelingPrepIntegration(configuration)
            .AddRedditIntegration(configuration)
            .AddFinnhubIntegration(configuration)
            .AddGoogleNewsIntegration()
            .AddOpenaiIntegration(configuration)
            .AddGeminiIntegration(configuration)
            .AddYahooIntegration(configuration);
        
        return services;
    }
}
