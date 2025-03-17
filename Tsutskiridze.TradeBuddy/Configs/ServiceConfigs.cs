using Telegram.Bot;
using Tsutskiridze.TradeBuddy.Services;
using Tsutskiridze.TradeBuddy.Services.AI;
using Tsutskiridze.TradeBuddy.Services.News;

namespace Tsutskiridze.TradeBuddy.Configs
{
    public static class ServiceConfigs
    {

        public static IServiceCollection AddServiceConfigs(this IServiceCollection services)
        {
            services.AddTransient<NewsService>();

            services.AddTransient<StockPromptService>();

            services.AddTransient<StockAnalysisService>();

            services.AddTransient<TelegramService>();

            services.AddTransient<IAIService, OpenAiService>();

            services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(SecretsManager.GetSecret("Telegram:BotToken")));
            return services;
        }
    }
}
