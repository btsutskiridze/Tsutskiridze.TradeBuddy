namespace Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock.Models
{
    public class StockAnalysisPrompt
    {
        public string AnalysisRequest { get; set; }
        public string InvestmentHorizon { get; set; }
        public StockAnalysisPromptDetails Stock { get; set; }
    }
}
