using Newtonsoft.Json;
using Tsutskiridze.TradeBuddy.DTOs.AlphaVantage;
using Tsutskiridze.TradeBuddy.Models.AlphaVantage;

namespace Tsutskiridze.TradeBuddy.Services.Stocks
{
    public class AlphaVantageService
    {
        private static readonly string _apiKey = SecretsManager.GetSecret("alphaVantage:apiKey");
        private readonly HttpClient _client;
        private readonly IMapper _mapper;

        public AlphaVantageService(HttpClient client, IMapper mapper)
        {
            _client = client;
            _mapper = mapper;
        }

        public async Task<AnnualReport?> GetStockLastAnnualReport(string symbol)
        {
            var response = await _client.GetAsync($"query?function=INCOME_STATEMENT&symbol={symbol}&apikey={_apiKey}");
            response.EnsureSuccessStatusCode();

            var data = JsonConvert.DeserializeObject<AnnualReportsResponse>(
                await response.Content.ReadAsStringAsync()
            );

            var report = data?.AnnualReports?.FirstOrDefault();

            return _mapper.Map<AnnualReport>(report);
        }

        public async Task<List<StockDayPrice>> GetStockPrevDaysClosePrices(string symbol, int days)
        {
            var response = await _client.GetAsync($"query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={_apiKey}");
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
