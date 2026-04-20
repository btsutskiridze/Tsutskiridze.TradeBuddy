using Microsoft.Extensions.DependencyInjection.Extensions;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Abstractions;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Commands;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Errors;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram;

public static class DependencyInjection
{
    public static IServiceCollection AddTelegramPresentation(this IServiceCollection services)
    {
        services.AddScoped<ITelegramCommandDispatcher, TelegramCommandDispatcher>();
        services.AddHostedService<TelegramCommandRegistrar>();

        var handlers = typeof(DependencyInjection).Assembly
            .GetTypes()
            .Where(x =>
                x is { IsAbstract: false, IsInterface: false } && typeof(ITelegramCommandHandler).IsAssignableFrom(x)
            );

        foreach (var handler in handlers)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient(typeof(ITelegramCommandHandler), handler
                ));
        }
        
        services.AddScoped<ITelegramErrorResponseFactory, TelegramErrorResponseFactory>();
        services.AddScoped<ITelegramCommandParser, TelegramCommandParser>();
        
        return services;
    }

}