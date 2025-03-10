using Newtonsoft.Json.Linq;
using Tsutskiridze.TradeBuddy.DTOs.Finnhub;

namespace Tsutskiridze.TradeBuddy.Services.News
{
    public class FinnhubService
    {

        private static readonly string _apiKey = SecretsManager.GetSecret("Finnhub:ApiKey");
        private readonly HttpClient _client;

        public FinnhubService(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<FinnhubNews>?> GetCompanyNewsAsync(string symbol, string from, string to, int? limit = null)
        {
            var response = await _client.GetAsync($"api/v1/company-news?symbol={symbol}&from={from}&to={to}&token={_apiKey}");
            response.EnsureSuccessStatusCode();

            var json = JArray.Parse(await response.Content.ReadAsStringAsync());

            if (json == null)
            {
                return null;
            }

            return json
                .Take(limit ?? 5)
                .Select(news =>
                {
                    return new FinnhubNews
                    {
                        Category = news["category"].Value<string>(),
                        Title = news["headline"].Value<string>(),
                        Source = news["source"].Value<string>(),
                        Summary = news["summary"].Value<string>(),
                        Url = news["url"].Value<string>(),
                        CreateTime = DateTimeOffset.FromUnixTimeSeconds(news["datetime"].Value<long>()).ToString("yyyy-MM-ddTHH:mm:ssZ")
                    };
                })
                .ToList();
        }

    }
}
