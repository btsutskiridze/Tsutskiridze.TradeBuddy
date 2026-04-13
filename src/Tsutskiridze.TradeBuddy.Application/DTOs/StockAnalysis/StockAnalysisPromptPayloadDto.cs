namespace Tsutskiridze.TradeBuddy.Application.DTOs.StockAnalysis
{
    public class StockAnalysisPromptPayloadDto
    {
        public string AnalysisRequest { get; set; }
        public string InvestmentHorizon { get; set; }
        public StockAnalysisInputDto Stock { get; set; }
    }
}
