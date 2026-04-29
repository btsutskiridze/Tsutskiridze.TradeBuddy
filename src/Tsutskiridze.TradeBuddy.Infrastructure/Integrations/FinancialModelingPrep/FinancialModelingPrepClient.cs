using System.Text.Json;
using Microsoft.Extensions.Options;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Common.Exceptions;
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

        public async Task<StockQuote?> GetStockQuote(string symbol)
        {
            var response = await _client.GetAsync($"api/v3/quote/{symbol}?apikey={_options.ApiKey}");
            response.EnsureSuccessStatusCode();

            var stockCurrentInfos = JsonSerializer.Deserialize<List<FmpStockQuoteResponse>?>(
                await response.Content.ReadAsStringAsync()
            );

            var stockQuote = stockCurrentInfos?.FirstOrDefault()?.ToDto()
                ?? throw new InfrastructureException("Failed to get stock quote");
            
            return stockQuote;
        }
    }
}

