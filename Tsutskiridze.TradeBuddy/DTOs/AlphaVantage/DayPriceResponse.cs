using Newtonsoft.Json;

namespace Tsutskiridze.TradeBuddy.DTOs.AlphaVantage
{
    public class DayPriceResponse
    {
        [JsonProperty("Time Series (Daily)")]
        public Dictionary<string, StockDayPriceDto> Prices { get; set; }
    }
}
