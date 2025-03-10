namespace Tsutskiridze.TradeBuddy.Models.AlphaVantage
{
    public class StockOverview
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal ReturnOnEquityTTM { get; set; }
        public decimal PriceToSalesRatioTTM { get; set; }
        public decimal QuarterlyRevenueGrowthYOY { get; set; }
    }

}
