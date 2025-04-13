using Tsutskiridze.TradeBuddy.Infrastructure.HttpClients.AlphaVantage;
using Tsutskiridze.TradeBuddy.Infrastructure.HttpClients.FinancialModelingPreg;
using Tsutskiridze.TradeBuddy.Infrastructure.HttpClients.Finnhub;
using Tsutskiridze.TradeBuddy.Infrastructure.HttpClients.Reddit;
using Tsutskiridze.TradeBuddy.Infrastructure.Scraping.Google;
using Tsutskiridze.TradeBuddy.Infrastructure.Scraping.Yahoo;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Configuration
{
    public static class HttpClientConfigs
    {

        public static IServiceCollection AddHttpClientConfigs(this IServiceCollection services)
        {
            services.AddHttpClient<AlphaVantageClient>(client =>
            {
                client.BaseAddress = new Uri(SecretsManager.GetSecret("AlphaVantage:BaseUrl"));
            });

            services.AddHttpClient<FinancialModelingPregClient>(client =>
            {
                client.BaseAddress = new Uri(SecretsManager.GetSecret("Fmp:BaseUrl"));
            });

            services.AddHttpClient<RedditClient>();

            services.AddHttpClient<FinnhubClient>(client =>
            {
                client.BaseAddress = new Uri(SecretsManager.GetSecret("Finnhub:BaseUrl"));
            });

            services.AddHttpClient<YahooStockScraper>(_configureYahooClient);

            services.AddHttpClient<YahooNewsScraper>(_configureYahooClient);

            services.AddHttpClient<GoogleScraper>();

            return services;
        }

        private static readonly Action<HttpClient> _configureYahooClient = client =>
        {
            client.BaseAddress = new Uri(SecretsManager.GetSecret("Yahoo:BaseUrl"));
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/web");
        };
    }
}
