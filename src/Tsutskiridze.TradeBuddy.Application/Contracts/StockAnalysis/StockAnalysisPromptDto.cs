namespace Tsutskiridze.TradeBuddy.Application.Contracts.StockAnalysis
{
    public class StockAnalysisPromptDto
    {
        public string AnalysisRequest { get; set; }
        public string InvestmentHorizon { get; set; }
        public StockAnalysisContextDto Stock { get; set; }
    }
}
