using Tsutskiridze.TradeBuddy.Application.Contracts.MarketData;
using Tsutskiridze.TradeBuddy.Application.Contracts.News;

namespace Tsutskiridze.TradeBuddy.Application.Contracts.StockAnalysis
{
    public class StockAnalysisContextDto : StockOverviewDto
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public StockQuoteDto? Quote { get; set; }
        public List<StockDayPriceDto>? PrevDays { get; set; }
        public AnnualReportDto? AnnualReport { get; set; }
        public StockNewsDto? News { get; set; }
    }
}
