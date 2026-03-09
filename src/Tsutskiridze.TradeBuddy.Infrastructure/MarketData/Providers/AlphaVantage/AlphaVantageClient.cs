using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;
using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.Mapping;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.AlphaVantage.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.AlphaVantage
{
    public class AlphaVantageClient : IAlphaVantageMarketDataProvider
    {
        private readonly AlphaVantageOptions _options;
        private readonly HttpClient _client;

        public AlphaVantageClient(HttpClient client, IOptions<AlphaVantageOptions> options)
        {
            _client = client;
            _options = options.Value;
        }

        public async Task<AnnualReportDto?> GetStockLastAnnualReport(string symbol)
        {
            var response = await _client.GetAsync($"query?function=INCOME_STATEMENT&symbol={symbol}&apikey={_options.ApiKey}");
            response.EnsureSuccessStatusCode();

            var data = JsonConvert.DeserializeObject<AlphaVantageAnnualReportsResponse>(
                await response.Content.ReadAsStringAsync()
            );

            var report = data?.AnnualReports?.FirstOrDefault();

            var annualReport = report.ToDto();

            return annualReport ?? throw new Exception("Failed to get stock annual report");
        }

        public async Task<StockOverviewDto?> GetStockOverview(string symbol)
        {
            var response = await _client.GetAsync($"query?function=OVERVIEW&symbol={symbol}&apikey={_options.ApiKey}");
            response.EnsureSuccessStatusCode();

            var stockOverviewResponse = JsonConvert.DeserializeObject<AlphaVantageStockOverviewResponse>(
                await response.Content.ReadAsStringAsync()
            );

            var stockOverview = stockOverviewResponse.ToDto();

            return stockOverview ?? throw new Exception("Failed to get stock overview");
        }

        public async Task<List<StockDayPriceDto>> GetStockPrevDaysClosePrices(string symbol, int? days = null)
        {
            var response = await _client.GetAsync($"query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={_options.ApiKey}");
            response.EnsureSuccessStatusCode();

            var data = JsonConvert.DeserializeObject<AlphaVantageDailyPricesResponse>(
                await response.Content.ReadAsStringAsync()
            );

            if (data?.Prices == null)
            {
                throw new Exception("Failed to get stock prices");
            }

            var prices = data.Prices
                .Take(days ?? data.Prices.Count)
                .Select(x => new StockDayPriceDto
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
