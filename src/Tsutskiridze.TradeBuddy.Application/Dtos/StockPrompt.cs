namespace Tsutskiridze.TradeBuddy.Application.Dtos
{
    public class StockPrompt
    {
        public string AnalysisRequest { get; set; }
        public string InvestmentHorizon { get; set; }
        public StockDetails Stock { get; set; }
    }
}
