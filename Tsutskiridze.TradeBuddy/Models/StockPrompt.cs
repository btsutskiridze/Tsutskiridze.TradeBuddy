namespace Tsutskiridze.TradeBuddy.Models
{
    public class StockPrompt
    {
        public string AnalysisRequest { get; set; }
        /*         
            {
                "currentPrice": "$2.56",
                "newsSentiment": "40% Positive",
                "aiAnalysis": "SELL (85% Confidence)",
                "reasoning":"your-reasoning-here"
            }
         */
        public string InvestmentHorizon { get; set; }
        public StockAnalysis ResponseStructure { get; set; }
        public Stock Stock { get; set; }
    }
}
