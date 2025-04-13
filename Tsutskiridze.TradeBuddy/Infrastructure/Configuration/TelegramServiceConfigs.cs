using Telegram.Bot;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Configuration
{
    public static class TelegramServiceConfigs
    {
        public static IServiceCollection AddTelegramServiceConfigs(this IServiceCollection services)
        {
            services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(SecretsManager.GetSecret("Telegram:BotToken")));

            services.AddTransient<ITelegramMessageHandler, StockHandler>();
            services.AddTransient<ITelegramMessageHandler, UnknownCommandHandler>();

            services.AddSingleton<ITelegramHandlerRegistry, TelegramHandlerRegistry>();
            services.AddTransient<TelegramWebhookService>();

            return services;
        }
    }
}
