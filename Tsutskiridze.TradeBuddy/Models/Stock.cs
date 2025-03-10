using Tsutskiridze.TradeBuddy.Models.AlphaVantage;
using Tsutskiridze.TradeBuddy.Models.Fmp;

namespace Tsutskiridze.TradeBuddy.Models
{
    public class Stock : StockOverview
    {
        public StockQuote? Quote { get; set; }
        public List<StockDayPrice>? PrevDays { get; set; }
        public AnnualReport? AnnualReport { get; set; }
    }
}
