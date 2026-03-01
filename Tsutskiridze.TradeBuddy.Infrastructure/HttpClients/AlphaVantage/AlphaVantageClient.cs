using AutoMapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tsutskiridze.TradeBuddy.Application.Abstractions;
using Tsutskiridze.TradeBuddy.Application.Dtos.AlphaVantage;
using Tsutskiridze.TradeBuddy.Core.Entities;

namespace Tsutskiridze.TradeBuddy.Infrastructure.HttpClients.AlphaVantage
{
    public class AlphaVantageClient : IAlphaVantageClient
    {
        private readonly AlphaVantageOptions _options;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;

        public AlphaVantageClient(HttpClient client, IMapper mapper, IOptions<AlphaVantageOptions> options)
        {
            _client = client;
            _mapper = mapper;
            _options = options.Value;
        }

        public async Task<AnnualReport?> GetStockLastAnnualReport(string symbol)
        {
            var response = await _client.GetAsync($"query?function=INCOME_STATEMENT&symbol={symbol}&apikey={_options.ApiKey}");
            response.EnsureSuccessStatusCode();

            var data = JsonConvert.DeserializeObject<AnnualReportsResponse>(
                await response.Content.ReadAsStringAsync()
            );

            var report = data?.AnnualReports?.FirstOrDefault();

            var annualReport = _mapper.Map<AnnualReport>(report);

            if (annualReport == null)
            {
                throw new Exception("Failed to get stock annual report");
            }

            return annualReport;
        }

        public async Task<StockOverview?> GetStockOverview(string symbol)
        {
            var response = await _client.GetAsync($"query?function=OVERVIEW&symbol={symbol}&apikey={_options.ApiKey}");
            response.EnsureSuccessStatusCode();

            var stockOverview = JsonConvert.DeserializeObject<StockOverview>(
                await response.Content.ReadAsStringAsync()
            );

            if (stockOverview == null)
            {
                throw new Exception("Failed to get stock overview");
            }

            return stockOverview;
        }

        public async Task<List<StockDayPrice>> GetStockPrevDaysClosePrices(string symbol, int? days = null)
        {
            var response = await _client.GetAsync($"query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={_options.ApiKey}");
            response.EnsureSuccessStatusCode();

            var data = JsonConvert.DeserializeObject<DayPriceResponse>(
                await response.Content.ReadAsStringAsync()
            );

            if (data?.Prices == null)
            {
                throw new Exception("Failed to get stock prices");
            }

            var prices = data.Prices
                .Take(days ?? data.Prices.Count)
                .Select(x => new StockDayPrice
                {
                    Date = DateTime.Parse(x.Key).ToString("yyyy-MM-dd"),
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
