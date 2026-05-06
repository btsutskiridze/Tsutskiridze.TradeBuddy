using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Abstractions;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Helpers;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Loading;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Web;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Api;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Api.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Parsing;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Parsing.Abstractions;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Streaming;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Streaming.Abstractions;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.News;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo;

public static class DependencyInjection
{
    public static IServiceCollection AddYahooIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<YahooOptions>(configuration.GetSection(YahooOptions.SectionName));

        services.AddCommonServices()
            .AddParsingServices()
            .AddStreamingServices()
            .AddApiServices();

        services.AddTransient<IYahooNewsProvider, YahooNewsProvider>();

        return services;
    }

    private static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        services.AddTransient<IYahooPayloadExtractor, YahooPayloadExtractor>();
        services.AddTransient<IYahooJsonNavigator, YahooJsonNavigator>();
        services.AddHttpClient<IYahooPageLoader, YahooPageLoader>(ConfigureYahooClient);
        services.AddTransient<IYahooCookieBypassService, YahooCookieBypassService>();

        return services;
    }
    
    private static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddHttpClient<IYahooHistoryApiProvider, YahooHistoryApiProvider>(ConfigureYahooApiClient);
        
        services.AddSingleton<YahooCookieJar>();
        services.AddHttpClient<IYahooStockQuoteApiProvider, YahooStockQuoteApiProvider>(ConfigureYahooApiClient)
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var cookieJar = sp.GetRequiredService<YahooCookieJar>();

                return new SocketsHttpHandler
                {
                    UseCookies = true,
                    CookieContainer = cookieJar.CookieContainer,
                    AutomaticDecompression =
                        DecompressionMethods.GZip |
                        DecompressionMethods.Deflate |
                        DecompressionMethods.Brotli
                };
            });
        
        return services;
    }

    private static IServiceCollection AddParsingServices(this IServiceCollection services)
    {
        services.AddTransient<IYahooQuotePageParser, YahooQuotePageParser>();
        services.AddTransient<IYahooHistoryPageParser, YahooHistoryPageParser>();
        services.AddTransient<IYahooKeyStatisticsPageParser, YahooKeyStatisticsPageParser>();
        services.AddTransient<IYahooFinancialsPageParser, YahooFinancialsPageParser>();

        return services;
    }

    private static IServiceCollection AddStreamingServices(this IServiceCollection services)
    {
        services.AddSingleton<IMarketDataTransportClient, YahooMarketDataTransportClient>();
        services.AddSingleton<IPricingMessageProcessor, PricingMessageProcessor>();
        services.AddSingleton<ISubscriptionManager, SubscriptionManager>();

        services.AddHostedService<StockPriceWebSocketListener>();

        return services;
    }
    
    private static readonly Action<IServiceProvider, HttpClient> ConfigureYahooApiClient = (serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<YahooOptions>>().Value;
        client.BaseAddress = new Uri(options.Query2ApiUrl);
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(15);
    };

    private static readonly Action<IServiceProvider, HttpClient> ConfigureYahooClient = (serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<YahooOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl);
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        client.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/web");
    };
}
