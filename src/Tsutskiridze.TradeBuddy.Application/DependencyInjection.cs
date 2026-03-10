using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.EventHandlers;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers;

namespace Tsutskiridze.TradeBuddy.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<StockPromptService>();
        services.AddTransient<StockAnalysisService>();
        services.AddSingleton<PricingUpdatedHandler>();

        RegisterTelegramCommandHandlers(services);
        services.AddSingleton<ITelegramHandlerRegistry, TelegramHandlerRegistry>();
        services.AddTransient<TelegramWebhookService>();

        return services;
    }

    private static void RegisterTelegramCommandHandlers(IServiceCollection services)
    {
        var handlers = Assembly.GetAssembly(typeof(ITelegramCommandHandler))!
            .GetTypes()
            .Where(type =>
                !type.IsInterface &&
                !type.IsAbstract &&
                typeof(ITelegramCommandHandler).IsAssignableFrom(type));

        foreach (var handler in handlers)
        {
            services.AddTransient(typeof(ITelegramCommandHandler), handler);
        }
    }
}
