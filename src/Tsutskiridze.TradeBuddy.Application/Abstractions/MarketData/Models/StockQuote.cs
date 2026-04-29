
using System.Text.Json.Serialization;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models
{
    public class StockQuote
    {
        [JsonIgnore]
        public string Symbol { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public string Currency { get; set; }

        public string Price { get; set; }
        public string ChangesPercentage { get; set; }
        public string Change { get; set; }
        public string DayLow { get; set; }
        public string DayHigh { get; set; }
        public string YearHigh { get; set; }
        public string YearLow { get; set; }
        public string MarketCap { get; set; }
        public string Exchange { get; set; }
        public string Volume { get; set; }
        public string AvgVolume { get; set; }
        public string Open { get; set; }
        public string PreviousClose { get; set; }
        public string Eps { get; set; }
        public string? Pe { get; set; }
        public string EarningsAnnouncement { get; set; }
        public string Timestamp { get; set; }
    }
}
