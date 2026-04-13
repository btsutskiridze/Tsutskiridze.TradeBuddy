using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;

namespace Tsutskiridze.TradeBuddy.Application.DTOs.StockAnalysis
{
    public class StockAnalysisInputDto : StockOverviewDto
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
