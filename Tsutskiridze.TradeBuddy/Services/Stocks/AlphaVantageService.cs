using Newtonsoft.Json;
using Tsutskiridze.Bloom.Core.Secrets;
using Tsutskiridze.TradeBuddy.DTOs.AlphaVantage;
using Tsutskiridze.TradeBuddy.Models.AlphaVantage;

namespace Tsutskiridze.TradeBuddy.Services.Stocks
{
    public class AlphaVantageService
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string ApiKey = SecretsManager.GetSecret("alphaVantage:apiKey");

        private const string BaseUrl = "https://www.alphavantage.co/query";

        public async Task<AnnualReportDto?> GetStockLastAnnualReport(string symbol)
        {
            string url = $"{BaseUrl}?function=INCOME_STATEMENT&symbol={symbol}&apikey={ApiKey}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var data = JsonConvert.DeserializeObject<AnnualReportsResponse>(
                await response.Content.ReadAsStringAsync()
            );

            return data?.AnnualReports.First();
        }

        public async Task<List<StockDayPrice>> GetStockPrevDaysClosePrices(string symbol, int days)
        {
            string url = $"{BaseUrl}?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={ApiKey}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var data = JsonConvert.DeserializeObject<DayPriceResponse>(
                await response.Content.ReadAsStringAsync()
            );

            if (data?.Prices == null)
            {
                return [];
            }

            var prices = data.Prices
                .Take(days)
                .Select(x => new StockDayPrice
                {
                    Date = DateTime.Parse(x.Key),
                    Open = x.Value.Open,
                    High = x.Value.High,
                    Low = x.Value.Low,
                    Close = x.Value.Close,
                    Volume = x.Value.Volume
                })
                .ToList();

            return prices;
        }
    }
}
