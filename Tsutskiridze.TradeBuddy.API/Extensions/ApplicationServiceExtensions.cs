using Tsutskiridze.TradeBuddy.Application.Features.NewsAggregation;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers;
using Tsutskiridze.TradeBuddy.Application.Mapping;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddTransient<NewsService>();

            services.AddTransient<StockPromptService>();

            services.AddTransient<StockAnalysisService>();

            services.AddTransient<ITelegramCommandHandler, QuoteCommandHandler>();
            services.AddTransient<ITelegramCommandHandler, UnknownCommandHandler>();

            services.AddSingleton<ITelegramHandlerRegistry, TelegramHandlerRegistry>();
            services.AddTransient<TelegramWebhookService>();

            services.AddAutoMapper(typeof(AutoMapperProfile));

            return services;
        }
    }
}
