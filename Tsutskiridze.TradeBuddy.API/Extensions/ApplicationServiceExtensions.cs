using Tsutskiridze.TradeBuddy.Application.Features.NewsAggregation;
using Tsutskiridze.TradeBuddy.Application.Features.StockAlerts;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Helpers;
using Tsutskiridze.TradeBuddy.Application.Mapping;
using Tsutskiridze.TradeBuddy.Infrastructure.Helpers;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddNewsServices()
                    .AddStockServices()
                    .AddTelegramServices();

            services.AddAutoMapper(typeof(AutoMapperProfile));
            services.AddMediator(cfg => cfg.ServiceLifetime = ServiceLifetime.Singleton);

            services.AddSingleton<ICurrencySymbolProvider, CurrencySymbolProvider>();

            // for single instance of SubscriptionManager
            services.AddSingleton<PriceChangeAlertService>();

            return services;
        }

        private static IServiceCollection AddNewsServices(this IServiceCollection services)
        {
            services.AddTransient<NewsService>();

            return services;
        }

        private static IServiceCollection AddStockServices(this IServiceCollection services)
        {
            services.AddTransient<StockPromptService>();
            services.AddTransient<StockAnalysisService>();

            return services;
        }

        private static IServiceCollection AddTelegramServices(this IServiceCollection services)
        {
            services.AddTransient<ITelegramCommandHandler, QuoteCommandHandler>();
            services.AddTransient<ITelegramCommandHandler, AlertCommandHandler>();
            services.AddTransient<ITelegramCommandHandler, UnknownCommandHandler>();

            services.AddSingleton<ITelegramHandlerRegistry, TelegramHandlerRegistry>();
            services.AddTransient<TelegramWebhookService>();

            return services;
        }

    }
}
