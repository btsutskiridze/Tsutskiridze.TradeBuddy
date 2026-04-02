using Newtonsoft.Json;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.AlphaVantage.Models
{
    public class AlphaVantageDailyPricesResponse
    {
        [JsonProperty("Time Series (Daily)")]
        public Dictionary<string, AlphaVantageStockDayPriceResponse> Prices { get; set; }
    }
}

