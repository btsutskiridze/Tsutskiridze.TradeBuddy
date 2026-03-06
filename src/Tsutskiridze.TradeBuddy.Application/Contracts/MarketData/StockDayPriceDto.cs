namespace Tsutskiridze.TradeBuddy.Application.Contracts.MarketData
{
    public class StockDayPriceDto
    {
        public string Date { get; set; }
        public string Open { get; set; }
        public string High { get; set; }
        public string Low { get; set; }
        public string Close { get; set; }
        public string Volume { get; set; }
    }
}
