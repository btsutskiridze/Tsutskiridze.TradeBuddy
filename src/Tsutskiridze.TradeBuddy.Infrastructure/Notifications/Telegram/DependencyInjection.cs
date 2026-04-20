using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Config;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;

public static class DependencyInjection
{
    public static IServiceCollection AddTelegramIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramClientOptions>(configuration.GetSection(TelegramClientOptions.SectionName));
        services.Configure<TelegramBotOptions>(configuration.GetSection(TelegramBotOptions.SectionName));
        
        services.AddSingleton<ITelegramBotClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<TelegramClientOptions>>().Value;
            return new TelegramBotClient(options.BotToken);
        });
        
        services.AddScoped<ITelegramSender, TelegramSender>();
        
        return services;
    }
    
}