using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Tsutskiridze.TradeBuddy.Application.Abstractions.AI;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.OpenAI.Schema;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.OpenAI;


public static class DependencyInjection
{
    public static IServiceCollection AddOpenaiIntegration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
            return new ChatClient(options.ModelID, options.ApiKey);
        });

        services.AddSingleton<IOpenAiJsonSchemaGenerator, OpenAiJsonSchemaGenerator>();
        services.AddTransient<IAiClient, OpenAiClient>();        
        
        return services;
    }
    
}