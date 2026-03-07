using Newtonsoft.Json;

namespace Tsutskiridze.TradeBuddy.Application.DTOs.Integrations.AlphaVantage
{
    public class AlphaVantageDailyPricesResponse
    {
        [JsonProperty("Time Series (Daily)")]
        public Dictionary<string, AlphaVantageStockDayPriceResponse> Prices { get; set; }
    }
}
