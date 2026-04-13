namespace Tsutskiridze.TradeBuddy.Application.DTOs.StockAnalysis
{
    public class StockAnalysisOutcomeDto
    {
        public string Symbol { get; set; }
        public StockAnalysisReportDto? Report { get; set; }
    }
}
