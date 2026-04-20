using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Utilities;
using Tsutskiridze.TradeBuddy.Infrastructure.Common.Data;
using Tsutskiridze.TradeBuddy.Infrastructure.Common.Messaging;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.AlphaVantage;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Finnhub;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Gemini;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.GoogleNews;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.OpenAI;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.News;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;
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
            .AddCommonServices()
            .AddIntegrations(configuration)
            .AddMarketDataServices()
            .AddNewsServices();

        return services;
    }
    
    private static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddSingleton<IDbExceptionClassifier, EfCoreDbExceptionClassifier>();

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
