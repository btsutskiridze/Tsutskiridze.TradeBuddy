using Tsutskiridze.TradeBuddy.Services;
using Tsutskiridze.TradeBuddy.Services.AI;
using Tsutskiridze.TradeBuddy.Services.News;
using Tsutskiridze.TradeBuddy.Services.Yahoo.Utilities;

namespace Tsutskiridze.TradeBuddy.Configs
{
    public static class ServiceConfigs
    {

        public static IServiceCollection AddServiceConfigs(this IServiceCollection services)
        {
            services.AddTransient<NewsService>();

            services.AddTransient<StockPromptService>();

            services.AddTransient<StockAnalysisService>();

            services.AddTransient<IAIService, OpenAiService>();

            services.AddTransient<IYahooCookieBypassService, YahooCookieBypassService>();

            return services;
        }
    }
}
