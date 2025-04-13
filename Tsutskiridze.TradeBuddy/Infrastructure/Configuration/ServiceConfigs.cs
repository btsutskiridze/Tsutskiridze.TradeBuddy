using Tsutskiridze.TradeBuddy.Application.Features.NewsAggregation;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis;
using Tsutskiridze.TradeBuddy.Application.Interfaces.AI;
using Tsutskiridze.TradeBuddy.Infrastructure.AI.OpenAI;
using Tsutskiridze.TradeBuddy.Infrastructure.Scraping.Yahoo.Utilities;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Configuration
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
