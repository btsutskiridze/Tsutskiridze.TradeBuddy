using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using SharedKernel;
using Telegram.Bot;
using Tsutskiridze.TradeBuddy.Application.Abstractions.AI;
using Tsutskiridze.TradeBuddy.Application.Abstractions.AI.OpenAI;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Streaming;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Utilities;
using Tsutskiridze.TradeBuddy.Application.Options;
using Tsutskiridze.TradeBuddy.Infrastructure.AI.Gemini;
using Tsutskiridze.TradeBuddy.Infrastructure.AI.OpenAI;
using Tsutskiridze.TradeBuddy.Infrastructure.AI.OpenAI.JsSchema;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.AlphaVantage;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.FinancialModelingPrep;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Streaming.Yahoo;
using Tsutskiridze.TradeBuddy.Infrastructure.News;
using Tsutskiridze.TradeBuddy.Infrastructure.News.Finnhub;
using Tsutskiridze.TradeBuddy.Infrastructure.News.GoogleNews;
using Tsutskiridze.TradeBuddy.Infrastructure.News.Reddit;
using Tsutskiridze.TradeBuddy.Infrastructure.News.Yahoo;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.CommandHandlers;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;
using Tsutskiridze.TradeBuddy.Infrastructure.Utilities;
using Tsutskiridze.TradeBuddy.Infrastructure.Utilities.Yahoo;

namespace Tsutskiridze.TradeBuddy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions(configuration)
            .AddPersistence(configuration)
            .AddUtilities()
            .AddTelegramNotificationServices()
            .AddExternalApiClients()
            .AddAiServices()
            .AddMarketDataServices()
            .AddBackgroundServices();

        return services;
    }

    private static IServiceCollection AddOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AlphaVantageOptions>(configuration.GetSection(AlphaVantageOptions.SectionName));
        services.Configure<FinancialModelingPrepOptions>(
            configuration.GetSection(FinancialModelingPrepOptions.SectionName));
        services.Configure<RedditOptions>(configuration.GetSection(RedditOptions.SectionName));
        services.Configure<FinnhubOptions>(configuration.GetSection(FinnhubOptions.SectionName));
        services.Configure<YahooOptions>(configuration.GetSection(YahooOptions.SectionName));
        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
        services.Configure<OpenAIOptions>(configuration.GetSection(OpenAIOptions.SectionName));
        services.Configure<TelegramClientOptions>(configuration.GetSection(TelegramClientOptions.SectionName));
        services.Configure<TelegramBotOptions>(configuration.GetSection(TelegramBotOptions.SectionName));

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                    configuration.GetConnectionString("Postgres"),
                    npgsql =>
                    {
                        npgsql.EnableRetryOnFailure();
                        npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                    })
                .UseSnakeCaseNamingConvention());

        services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }

    private static IServiceCollection AddExternalApiClients(this IServiceCollection services)
    {
        services.AddHttpClient<IAlphaVantageMarketDataProvider, AlphaVantageClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<AlphaVantageOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        services.AddHttpClient<IFinancialModelingPrepQuoteProvider, FinancialModelingPrepClient>((serviceProvider,
            client) =>
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
        services.AddHttpClient<IYahooMarketDataProvider, YahooStockScraper>(ConfigureYahooClient);
        services.AddHttpClient<IYahooNewsProvider, YahooNewsScraper>(ConfigureYahooClient);
        services.AddTransient<IYahooCookieBypassService, YahooCookieBypassService>();

        services.AddTransient<INewsAggregator, NewsAggregator>();

        return services;
    }

    private static IServiceCollection AddAiServices(this IServiceCollection services)
    {
        services.AddSingleton<ITelegramBotClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<TelegramClientOptions>>().Value;
            return new TelegramBotClient(options.BotToken);
        });

        services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            return new ChatClient(options.ModelID, options.ApiKey);
        });

        services.AddSingleton<IOpenaiJsSchemaGenerator, OpenaiJsSchemaGenerator>();
        services.AddTransient<IAiService, OpenAIService>();

        return services;
    }

    private static IServiceCollection AddMarketDataServices(this IServiceCollection services)
    {
        services.AddSingleton<IMarketDataTransportClient, YahooMarketDataTransportClient>();
        services.AddSingleton<IPricingMessageProcessor, PricingMessageProcessor>();

        services.AddSingleton<SubscriptionManager>();
        services.AddSingleton<ISubscriptionManager>(serviceProvider =>
            serviceProvider.GetRequiredService<SubscriptionManager>());

        return services;
    }

    private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<StockPriceWebSocketListener>();

        return services;
    }

    private static IServiceCollection AddUtilities(this IServiceCollection services)
    {
        services.AddSingleton<ICurrencySymbolProvider, CurrencySymbolProvider>();
        services.AddSingleton<IDbExceptionClassifier, EfCoreDbExceptionClassifier>();

        return services;
    }

    private static IServiceCollection AddTelegramNotificationServices(this IServiceCollection services)
    {
        services.AddScoped<ITelegramSender, TelegramSender>();
        services.AddScoped<ITelegramWebhookRouter, TelegramWebhookRouter>();
        services.AddHostedService<TelegramCommandsRegistrationHostedService>();

        var handlers = typeof(DependencyInjection).Assembly
            .GetTypes()
            .Where(x =>
                x is { IsAbstract: false, IsInterface: false } && typeof(ITelegramCommandHandler).IsAssignableFrom(x)
            );

        foreach (var handler in handlers)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient(typeof(ITelegramCommandHandler), handler
            ));
        }
        
        return services;
    }


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