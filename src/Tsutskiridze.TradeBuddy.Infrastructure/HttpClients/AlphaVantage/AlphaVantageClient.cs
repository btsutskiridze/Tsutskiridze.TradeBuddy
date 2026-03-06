using AutoMapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tsutskiridze.TradeBuddy.Application.Abstractions;
using Tsutskiridze.TradeBuddy.Application.Contracts.Integrations.AlphaVantage;
using Tsutskiridze.TradeBuddy.Application.Contracts.MarketData;

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

        public async Task<AnnualReportDto?> GetStockLastAnnualReport(string symbol)
        {
            var response = await _client.GetAsync($"query?function=INCOME_STATEMENT&symbol={symbol}&apikey={_options.ApiKey}");
            response.EnsureSuccessStatusCode();

            var data = JsonConvert.DeserializeObject<AlphaVantageAnnualReportsResponse>(
                await response.Content.ReadAsStringAsync()
            );

            var report = data?.AnnualReports?.FirstOrDefault();

            var annualReport = _mapper.Map<AnnualReportDto>(report);

            if (annualReport == null)
            {
                throw new Exception("Failed to get stock annual report");
            }

            return annualReport;
        }

        public async Task<StockOverviewDto?> GetStockOverview(string symbol)
        {
            var response = await _client.GetAsync($"query?function=OVERVIEW&symbol={symbol}&apikey={_options.ApiKey}");
            response.EnsureSuccessStatusCode();

            var stockOverviewResponse = JsonConvert.DeserializeObject<AlphaVantageStockOverviewResponse>(
                await response.Content.ReadAsStringAsync()
            );

            var stockOverview = _mapper.Map<StockOverviewDto>(stockOverviewResponse);

            if (stockOverview == null)
            {
                throw new Exception("Failed to get stock overview");
            }

            return stockOverview;
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
