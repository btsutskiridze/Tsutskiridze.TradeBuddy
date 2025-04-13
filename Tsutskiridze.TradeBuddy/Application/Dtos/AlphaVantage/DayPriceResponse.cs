using Newtonsoft.Json;

namespace Tsutskiridze.TradeBuddy.Application.Dtos.AlphaVantage
{
    public class DayPriceResponse
    {
        [JsonProperty("Time Series (Daily)")]
        public Dictionary<string, StockDayPriceDto> Prices { get; set; }
    }
}
