using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Http;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram.Config;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram;

public static class DependencyInjection
{
    public static IServiceCollection AddTelegramIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramClientOptions>(configuration.GetSection(TelegramClientOptions.SectionName));
        services.Configure<TelegramBotOptions>(configuration.GetSection(TelegramBotOptions.SectionName));

        services.AddHttpClient(TelegramClientOptions.ResiliencePipelineName)
            .AddResiliencePipeline(TelegramClientOptions.ResiliencePipelineName);
        
        services.AddSingleton<ITelegramBotClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<TelegramClientOptions>>().Value;
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(TelegramClientOptions.ResiliencePipelineName);
            
            return new TelegramBotClient(options.BotToken, httpClient);
        });
        
        services.AddScoped<ITelegramSender, TelegramSender>();
        
        return services;
    }
    
}
