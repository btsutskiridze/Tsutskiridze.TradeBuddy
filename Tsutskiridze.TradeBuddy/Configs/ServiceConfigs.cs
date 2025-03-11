using Tsutskiridze.TradeBuddy.Services;
using Tsutskiridze.TradeBuddy.Services.News;

namespace Tsutskiridze.TradeBuddy.Configs
{
    public static class ServiceConfigs
    {

        public static IServiceCollection AddServiceConfigs(this IServiceCollection services)
        {
            services.AddTransient<NewsService>();

            services.AddTransient<StockPromptService>();

            return services;
        }
    }
}
