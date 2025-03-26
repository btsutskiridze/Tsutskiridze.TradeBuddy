using Telegram.Bot;
using Tsutskiridze.TradeBuddy.Services.Telegram;
using Tsutskiridze.TradeBuddy.Services.Telegram.MessageHandlers;

namespace Tsutskiridze.TradeBuddy.Configs
{
    public static class TelegramServiceConfigs
    {
        public static IServiceCollection AddTelegramServiceConfigs(this IServiceCollection services)
        {
            services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(SecretsManager.GetSecret("Telegram:BotToken")));

            services.AddTransient<ITelegramMessageHandler, StockHandler>();
            services.AddTransient<ITelegramMessageHandler, UnknownCommandHandler>();

            services.AddSingleton<ITelegramHandlerRegistry, TelegramHandlerRegistry>();
            services.AddTransient<TelegramService>();

            return services;
        }
    }
}
