using AutoMapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tsutskiridze.TradeBuddy.Application.Abstractions;
using Tsutskiridze.TradeBuddy.Application.Dtos;
using Tsutskiridze.TradeBuddy.Application.Dtos.Fmp;

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

        public async Task<StockQuote?> GetStockQuote(string symbol)
        {
            var response = await _client.GetAsync($"api/v3/quote/{symbol}?apikey={_options.ApiKey}");
            response.EnsureSuccessStatusCode();

            var stockCurrentInfos = JsonConvert.DeserializeObject<List<StockQuoteDto>?>(
                await response.Content.ReadAsStringAsync()
            );

            var stockQuote = _mapper.Map<StockQuote>(stockCurrentInfos?.FirstOrDefault());

            if (stockQuote == null)
            {
                throw new Exception("Failed to get stock quote");
            }

            return stockQuote;
        }
    }
}
