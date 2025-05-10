using Tsutskiridze.TradeBuddy.Application.Features.NewsAggregation;
using Tsutskiridze.TradeBuddy.Application.Features.StockAlerts;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Helpers;
using Tsutskiridze.TradeBuddy.Application.Mapping;
using Tsutskiridze.TradeBuddy.Infrastructure.Helpers;

namespace Tsutskiridze.TradeBuddy.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services
                .AddCoreApplicationServices()
                .AddFeatureServices()
                .AddHelperServices();

            return services;
        }

        private static IServiceCollection AddCoreApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(AutoMapperProfile));
            services.AddMediator(cfg => cfg.ServiceLifetime = ServiceLifetime.Singleton);

            return services;
        }

        private static IServiceCollection AddFeatureServices(this IServiceCollection services)
        {
            // News Services
            services.AddTransient<NewsService>();

            // Stock Services
            services.AddTransient<StockPromptService>();
            services.AddTransient<StockAnalysisService>();
            services.AddSingleton<PriceChangeAlertService>();

            // Telegram Services
            services.AddTransient<ITelegramCommandHandler, QuoteCommandHandler>();
            services.AddTransient<ITelegramCommandHandler, AlertCommandHandler>();
            services.AddTransient<ITelegramCommandHandler, MyAlertsCommandHandler>();
            services.AddTransient<ITelegramCommandHandler, RemoveAlertCommandHandler>();
            services.AddTransient<ITelegramCommandHandler, UnknownCommandHandler>();
            services.AddSingleton<ITelegramHandlerRegistry, TelegramHandlerRegistry>();
            services.AddTransient<TelegramWebhookService>();

            return services;
        }

        private static IServiceCollection AddHelperServices(this IServiceCollection services)
        {
            services.AddSingleton<ICurrencySymbolProvider, CurrencySymbolProvider>();

            return services;
        }
    }
}
