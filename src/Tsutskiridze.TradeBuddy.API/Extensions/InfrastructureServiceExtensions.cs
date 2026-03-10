using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Telegram.Bot;
using Tsutskiridze.TradeBuddy.Application.Abstractions.AI;
using Tsutskiridze.TradeBuddy.Application.Abstractions.AI.OpenAI;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Streaming;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Utilities;
using Tsutskiridze.TradeBuddy.Infrastructure.AI.OpenAI;
using Tsutskiridze.TradeBuddy.Infrastructure.AI.OpenAI.JsSchema;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.AlphaVantage;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.FinancialModelingPrep;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Streaming.Yahoo;
using Tsutskiridze.TradeBuddy.Infrastructure.News.Finnhub;
using Tsutskiridze.TradeBuddy.Infrastructure.News.GoogleNews;
using Tsutskiridze.TradeBuddy.Infrastructure.News.Reddit;
using Tsutskiridze.TradeBuddy.Infrastructure.News.Yahoo;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Infrastructure.Utilities;
using Tsutskiridze.TradeBuddy.Infrastructure.Utilities.Yahoo;

namespace Tsutskiridze.TradeBuddy.API.Extensions
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services
                .AddHelperServices()
                .AddExternalApiClients()
                .AddAIServices()
                .AddStockMarketServices()
                .AddBackgroundServices();

            return services;
        }

        private static IServiceCollection AddExternalApiClients(this IServiceCollection services)
        {
            // Financial Data Providers
            services.AddHttpClient<IAlphaVantageMarketDataProvider, AlphaVantageClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<AlphaVantageOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });

            services.AddHttpClient<IFinancialModelingPrepQuoteProvider, FinancialModelingPrepClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<FinancialModelingPrepOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });

            // News Providers
            services.AddHttpClient<IRedditNewsProvider, RedditClient>();
            services.AddHttpClient<IFinnhubNewsProvider, FinnhubClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<FinnhubOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });

            // Scraping Services
            services.AddHttpClient<IGoogleNewsProvider, GoogleScraper>();
            services.AddHttpClient<IYahooMarketDataProvider, YahooStockScraper>(configureYahooClient);
            services.AddHttpClient<IYahooNewsProvider, YahooNewsScraper>(configureYahooClient);
            services.AddTransient<IYahooCookieBypassService, YahooCookieBypassService>();

            return services;
        }

        private static IServiceCollection AddAIServices(this IServiceCollection services)
        {
            services.AddSingleton<ITelegramBotClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<TelegramClientOptions>>().Value;
                return new TelegramBotClient(options.BotToken);
            });

            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<OpenAIOptions>>().Value;
                return new ChatClient(options.ModelID, options.ApiKey);
            });

            services.AddSingleton<IOpenaiJsSchemaGenerator, OpenaiJsSchemaGenerator>();
            services.AddTransient<IAiService, OpenAIService>();

            return services;
        }

        private static IServiceCollection AddStockMarketServices(this IServiceCollection services)
        {
            services.AddSingleton<IMarketDataTransportClient, YahooMarketDataTransportClient>();
            services.AddSingleton<IPricingMessageProcessor, PricingMessageProcessor>();

            //Mediator Single instance
            services.AddSingleton<SubscriptionManager>();
            services.AddSingleton<ISubscriptionManager>(sp => sp.GetRequiredService<SubscriptionManager>());

            return services;
        }

        private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<TelegramCommandsRegisterService>();
            services.AddHostedService<StockPriceWebSocketListener>();

            return services;
        }


        private static IServiceCollection AddHelperServices(this IServiceCollection services)
        {
            services.AddSingleton<ICurrencySymbolProvider, CurrencySymbolProvider>();

            services.AddScoped<ITelegramSender, TelegramSender>();
            services.AddScoped<ITelegramWebhookRouter, TelegramWebhookRouter>();

            return services;
        }

        private static readonly Action<IServiceProvider, HttpClient> configureYahooClient = (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<YahooOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            client.DefaultRequestHeaders.Add("Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/web");
        };
    }
}