using Newtonsoft.Json;
using Tsutskiridze.TradeBuddy.Application.Dtos.Fmp;
using Tsutskiridze.TradeBuddy.Models.Fmp;

namespace Tsutskiridze.TradeBuddy.Infrastructure.HttpClients.FinancialModelingPreg
{
    public class FinancialModelingPregClient
    {
        private static readonly string _apiKey = SecretsManager.GetSecret("Fmp:ApiKey");
        private readonly HttpClient _client;
        private readonly IMapper _mapper;

        public FinancialModelingPregClient(HttpClient client, IMapper mapper)
        {
            _client = client;
            _mapper = mapper;
        }

        public async Task<StockQuote?> GetStockQuote(string symbol)
        {
            var response = await _client.GetAsync($"api/v3/quote/{symbol}?apikey={_apiKey}");
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
