using Tsutskiridze.TradeBuddy.Services.Stocks;

namespace Tsutskiridze.TradeBuddy.Configs
{
    public static class HttpClientConfigs
    {

        public static IServiceCollection AddHttpClientConfigs(this IServiceCollection services)
        {
            services.AddHttpClient<AlphaVantageService>(client =>
            {
                client.BaseAddress = new Uri(SecretsManager.GetSecret("AlphaVantage:BaseUrl"));
            });

            services.AddHttpClient<FMPService>(client =>
            {
                client.BaseAddress = new Uri(SecretsManager.GetSecret("Fmp:BaseUrl"));
            });

            return services;
        }

    }
}
