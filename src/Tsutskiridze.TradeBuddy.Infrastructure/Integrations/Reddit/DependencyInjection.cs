using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Http;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit;

public static class DependencyInjection
{
    public static IServiceCollection AddRedditIntegration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedditOptions>(configuration.GetSection(RedditOptions.SectionName));

        services.AddHttpClient<IRedditNewsProvider, RedditNewsProvider>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<RedditOptions>>().Value;
            client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
        })
        .AddResiliencePipeline(RedditOptions.ResiliencePipelineName);

        return services;
    }
}
