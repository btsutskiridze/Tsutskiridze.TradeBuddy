using AutoMapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tsutskiridze.TradeBuddy.Application.Abstractions;
using Tsutskiridze.TradeBuddy.Application.Contracts.Integrations.FinancialModelingPrep;
using Tsutskiridze.TradeBuddy.Application.Contracts.MarketData;

namespace Tsutskiridze.TradeBuddy.Infrastructure.HttpClients.FinancialModelingPreg
{
    public class FinancialModelingPrepClient : IFinancialModelingPrepClient
    {
        private readonly FinancialModelingPrepOptions _options;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;

        public FinancialModelingPrepClient(HttpClient client, IMapper mapper, IOptions<FinancialModelingPrepOptions> options)
        {
            _client = client;
            _mapper = mapper;
            _options = options.Value;
        }

        public async Task<StockQuoteDto?> GetStockQuote(string symbol)
        {
            var response = await _client.GetAsync($"api/v3/quote/{symbol}?apikey={_options.ApiKey}");
            response.EnsureSuccessStatusCode();

            var stockCurrentInfos = JsonConvert.DeserializeObject<List<FmpStockQuoteResponse>?>(
                await response.Content.ReadAsStringAsync()
            );

            var stockQuote = _mapper.Map<StockQuoteDto>(stockCurrentInfos?.FirstOrDefault());

            if (stockQuote == null)
            {
                throw new Exception("Failed to get stock quote");
            }

            return stockQuote;
        }
    }
}
