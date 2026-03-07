namespace Tsutskiridze.TradeBuddy.Application.DTOs.Integrations.AlphaVantage
{
    public class AlphaVantageStockOverviewResponse
    {
        public string ReturnOnEquityTTM { get; set; }
        public string PriceToSalesRatioTTM { get; set; }
        public string QuarterlyRevenueGrowthYOY { get; set; }
        public string PriceAvg50 { get; set; }
        public string PriceAvg200 { get; set; }
        public string SharesOutstanding { get; set; }
    }
}
