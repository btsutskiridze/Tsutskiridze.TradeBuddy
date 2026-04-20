using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Gemini;

public static class DependencyInjection
{
    public static IServiceCollection AddGeminiIntegration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
        // services.AddTransient<IAiClient, GeminiClient>();
        
        return services;
    }
    
}