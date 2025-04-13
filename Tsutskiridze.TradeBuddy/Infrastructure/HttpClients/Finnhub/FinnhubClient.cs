using Newtonsoft.Json.Linq;
using Tsutskiridze.TradeBuddy.Application.Dtos.Finnhub;

namespace Tsutskiridze.TradeBuddy.Infrastructure.HttpClients.Finnhub
{
    public class FinnhubClient
    {

        private static readonly string _apiKey = SecretsManager.GetSecret("Finnhub:ApiKey");
        private readonly HttpClient _client;

        public FinnhubClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<FinnhubNews>?> GetCompanyNewsAsync(string symbol, DateTime from, DateTime to, int? limit = null)
        {
            var response = await _client.GetAsync($"api/v1/company-news?symbol={symbol}&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}&token={_apiKey}");
            response.EnsureSuccessStatusCode();

            var json = JArray.Parse(await response.Content.ReadAsStringAsync());

            if (json == null)
            {
                return null;
            }

            var newsList = new List<FinnhubNews>();

            foreach (var item in json)
            {
                try
                {

                    newsList.Add(new FinnhubNews
                    {
                        Category = item["category"].Value<string>(),
                        Title = item["headline"].Value<string>(),
                        Source = item["source"].Value<string>(),
                        Summary = item["summary"].Value<string>(),
                        Url = item["url"].Value<string>(),
                        CreateTime = DateTimeOffset.FromUnixTimeSeconds(item["datetime"].Value<long>()).ToString("yyyy-MM-ddTHH:mm:ssZ")
                    });
                }
                catch (Exception)
                {
                }

                if (newsList.Count >= limit)
                {
                    break;
                }
            }

            return newsList.Count > 0 ? newsList : null;
        }

    }
}
