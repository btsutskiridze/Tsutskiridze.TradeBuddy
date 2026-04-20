using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit;

public static class DependencyInjection
{
    public static IServiceCollection AddRedditIntegration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedditOptions>(configuration.GetSection(RedditOptions.SectionName));
        
        services.AddHttpClient<IRedditNewsProvider, RedditNewsProvider>();
        
        return services;
    }
}