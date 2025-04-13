using Tsutskiridze.TradeBuddy.Application.Dtos;
using Tsutskiridze.TradeBuddy.Models.AlphaVantage;
using Tsutskiridze.TradeBuddy.Models.Fmp;

namespace Tsutskiridze.TradeBuddy.Models
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
