using Newtonsoft.Json;
using Tsutskiridze.TradeBuddy.DTOs.Fmp;
using Tsutskiridze.TradeBuddy.Models.Fmp;

namespace Tsutskiridze.TradeBuddy.Services.Stocks
{
    public class FMPService
    {
        private const string _baseUrl = "https://financialmodelingprep.com/api";
        private static readonly string _apiKey = SecretsManager.GetSecret("Fmp:ApiKey");
        private readonly HttpClient _client;
        private readonly IMapper _mapper;

        public FMPService(HttpClient client, IMapper mapper)
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

            return _mapper.Map<StockQuote>(stockCurrentInfos?.FirstOrDefault());
        }
    }
}
