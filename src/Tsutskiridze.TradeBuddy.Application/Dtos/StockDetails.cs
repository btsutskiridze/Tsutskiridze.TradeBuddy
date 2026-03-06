namespace Tsutskiridze.TradeBuddy.Application.Dtos
{
    public class StockDetails : StockOverview
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public StockQuote? Quote { get; set; }
        public List<StockDayPrice>? PrevDays { get; set; }
        public AnnualReport? AnnualReport { get; set; }
        public AllNews? News { get; set; }
    }
}
