using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.Contracts.News;

namespace Tsutskiridze.TradeBuddy.Infrastructure.HttpClients.Finnhub
{
    public class FinnhubClient : IFinnhubNewsProvider
    {
        private readonly FinnhubOptions _options;
        private readonly HttpClient _client;

        public FinnhubClient(HttpClient client, IOptions<FinnhubOptions> options)
        {
            _options = options.Value;
            _client = client;
        }

        public async Task<List<FinnhubNewsItemDto>?> GetCompanyNewsAsync(string symbol, DateTime from, DateTime to, int? limit = null)
        {
            var response = await _client.GetAsync($"api/v1/company-news?symbol={symbol}&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}&token={_options.ApiKey}");
            response.EnsureSuccessStatusCode();

            var json = JArray.Parse(await response.Content.ReadAsStringAsync());

            if (json == null)
            {
                return null;
            }

            var newsList = new List<FinnhubNewsItemDto>();

            foreach (var item in json)
            {
                try
                {

                    newsList.Add(new FinnhubNewsItemDto
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
