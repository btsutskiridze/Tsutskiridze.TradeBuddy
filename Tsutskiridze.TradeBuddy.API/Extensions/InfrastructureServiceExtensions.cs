using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Telegram.Bot;
using Tsutskiridze.TradeBuddy.Application.Interfaces;
using Tsutskiridze.TradeBuddy.Application.Interfaces.AI;
using Tsutskiridze.TradeBuddy.Application.Interfaces.AI.OpenAI;
using Tsutskiridze.TradeBuddy.Application.Interfaces.News;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Yahoo;
using Tsutskiridze.TradeBuddy.Infrastructure.AI.OpenAI;
using Tsutskiridze.TradeBuddy.Infrastructure.AI.OpenAI.JsSchema;
using Tsutskiridze.TradeBuddy.Infrastructure.HttpClients.AlphaVantage;
using Tsutskiridze.TradeBuddy.Infrastructure.HttpClients.FinancialModelingPreg;
using Tsutskiridze.TradeBuddy.Infrastructure.HttpClients.Finnhub;
using Tsutskiridze.TradeBuddy.Infrastructure.HttpClients.Reddit;
using Tsutskiridze.TradeBuddy.Infrastructure.Scraping.Google;
using Tsutskiridze.TradeBuddy.Infrastructure.Scraping.Yahoo;
using Tsutskiridze.TradeBuddy.Infrastructure.Scraping.Yahoo.Utilities;
using Tsutskiridze.TradeBuddy.Infrastructure.Telegram;

namespace Tsutskiridze.TradeBuddy.API.Extensions
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddHttpClient<IAlphaVantageClient, AlphaVantageClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AlphaVantageOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });

            services.AddHttpClient<IFinancialModelingPrepClient, FinancialModelingPrepClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<FinancialModelingPrepOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });

            services.AddHttpClient<IRedditNewsProvider, RedditClient>();

            services.AddHttpClient<IFinnhubNewsProvider, FinnhubClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<FinnhubOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });

            services.AddHttpClient<IGoogleNewsProvider, GoogleScraper>();

            services.AddHttpClient<IYahooStockScraper, YahooStockScraper>(configureYahooClient);
            services.AddHttpClient<IYahooNewsProvider, YahooNewsScraper>(configureYahooClient);
            services.AddTransient<IYahooCookieBypassService, YahooCookieBypassService>();

            services.AddSingleton<ITelegramBotClient>((serviceProvider) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<TelegramClientOptions>>().Value;
                return new TelegramBotClient(options.BotToken);
            });

            services.AddSingleton((serviceProvider) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
                return new ChatClient(options.ModelID, options.ApiKey);
            });
            services.AddSingleton<IOpenaiJsSchemaGenerator, OpenaiJsSchemaGenerator>();
            services.AddTransient<IAIService, OpenAIService>();

            services.AddHostedServices();

            return services;
        }

        private static IServiceCollection AddHostedServices(this IServiceCollection services)
        {
            services.AddHostedService<TelegramCommandsRegisterService>();
            return services;
        }



        private static readonly Action<IServiceProvider, HttpClient> configureYahooClient = (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<YahooOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/web");
        };
    }
}
