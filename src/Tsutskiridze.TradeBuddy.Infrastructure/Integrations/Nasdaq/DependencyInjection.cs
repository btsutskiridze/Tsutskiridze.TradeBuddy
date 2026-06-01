using Microsoft.Extensions.DependencyInjection;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Http;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Nasdaq.SymbolDirectory;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Nasdaq;

public static class DependencyInjection
{
    public static IServiceCollection AddNasdaqIntegration(this IServiceCollection services)
    {
        services.AddHttpClient(NasdaqOptions.SymbolDirectoryClientName, client =>
            {
                client.BaseAddress = new Uri("https://www.nasdaqtrader.com/");
                client.DefaultRequestHeaders.Add("Accept", "text/plain");
                client.DefaultRequestHeaders.Add(
                    "User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            })
            .AddResiliencePipeline(NasdaqOptions.SymbolDirectoryResiliencePipelineName);

        services.AddSingleton<INasdaqSymbolDirectoryProvider, NasdaqSymbolDirectoryProvider>();

        return services;
    }
}
