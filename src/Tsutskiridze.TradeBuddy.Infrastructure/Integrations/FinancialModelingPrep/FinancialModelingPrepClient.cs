using System.Text.Json;
using Microsoft.Extensions.Options;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Exceptions;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep.Mapping;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Serialization;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep
{
    public class FinancialModelingPrepClient : IFinancialModelingPrepQuoteProvider
    {
        private readonly FinancialModelingPrepOptions _options;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public FinancialModelingPrepClient(
            HttpClient client,
            IOptions<FinancialModelingPrepOptions> options,
            InfraJsonSerializerOptions jsonSerializerOptions)
        {
            _client = client;
            _options = options.Value;
            _jsonSerializerOptions = jsonSerializerOptions.Options;
        }

        public async Task<StockQuote?> GetStockQuote(string symbol)
        {
            var response = await _client.GetAsync($"api/v3/quote/{symbol}?apikey={_options.ApiKey}");
            response.EnsureSuccessStatusCode();

            var stockCurrentInfos = JsonSerializer.Deserialize<List<FmpStockQuoteResponse>?>(
                await response.Content.ReadAsStringAsync(),
                _jsonSerializerOptions);

            var stockQuote = stockCurrentInfos?.FirstOrDefault()?.ToDto()
                ?? throw new InfrastructureException("Failed to get stock quote");
            
            return stockQuote;
        }
    }
}

