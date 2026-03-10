using System.Reflection;
using SharedKernel.Validations.Mediator;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.EventHandlers;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers;

namespace Tsutskiridze.TradeBuddy.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services
                .AddCoreApplicationServices()
                .AddFeatureServices();

            return services;
        }

        private static IServiceCollection AddCoreApplicationServices(this IServiceCollection services)
        {
            services
                .AddMediator(opt =>
                {
                    opt.ServiceLifetime = ServiceLifetime.Scoped;
                    opt.PipelineBehaviors = [typeof(ValidatorBehavior<,>)];
                })
                .AddMediatorValidators(typeof(Application.AssemblyReference).Assembly);
            return services;
        }

        private static IServiceCollection AddFeatureServices(this IServiceCollection services)
        {
            // Stock Services
            services.AddTransient<StockPromptService>();
            services.AddTransient<StockAnalysisService>();
            services.AddSingleton<PricingUpdatedHandler>();

            // Telegram Services
            services.AddTelegramCommandHandlers();
            services.AddSingleton<ITelegramHandlerRegistry, TelegramHandlerRegistry>();
            services.AddTransient<TelegramWebhookService>();

            return services;
        }

        private static IServiceCollection AddTelegramCommandHandlers(this IServiceCollection services)
        {
            var handlers = Assembly.GetAssembly(typeof(ITelegramCommandHandler))!
                .GetTypes()
                .Where(t => !t.IsInterface && !t.IsAbstract && typeof(ITelegramCommandHandler).IsAssignableFrom(t));

            foreach (var handler in handlers)
            {
                services.AddTransient(typeof(ITelegramCommandHandler), handler);
            }

            return services;
        }
    }
}
