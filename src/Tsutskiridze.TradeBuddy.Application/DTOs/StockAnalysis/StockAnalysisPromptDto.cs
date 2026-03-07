namespace Tsutskiridze.TradeBuddy.Application.DTOs.StockAnalysis
{
    public class StockAnalysisPromptDto
    {
        public string AnalysisRequest { get; set; }
        public string InvestmentHorizon { get; set; }
        public StockAnalysisContextDto Stock { get; set; }
    }
}
