using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;
using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep.Mapping;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep
{
    public class FinancialModelingPrepClient : IFinancialModelingPrepQuoteProvider
    {
        private readonly FinancialModelingPrepOptions _options;
        private readonly HttpClient _client;

        public FinancialModelingPrepClient(HttpClient client, IOptions<FinancialModelingPrepOptions> options)
        {
            _client = client;
            _options = options.Value;
        }

        public async Task<StockQuoteDto?> GetStockQuote(string symbol)
        {
            var response = await _client.GetAsync($"api/v3/quote/{symbol}?apikey={_options.ApiKey}");
            response.EnsureSuccessStatusCode();

            var stockCurrentInfos = JsonConvert.DeserializeObject<List<FmpStockQuoteResponse>?>(
                await response.Content.ReadAsStringAsync()
            );

            var stockQuote = stockCurrentInfos?.FirstOrDefault()?.ToDto();

            return stockQuote ?? throw new Exception("Failed to get stock quote");
        }
    }
}

