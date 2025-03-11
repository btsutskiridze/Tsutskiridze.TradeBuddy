using Tsutskiridze.TradeBuddy.Models;

namespace Tsutskiridze.TradeBuddy.Helpers
{
    public static class StockPromptParams
    {
        //public static readonly string AnalysisRequest = "Analyze the given stock data and market sentiment to determine if now is a good time to BUY, SELL, or HOLD RCAT. Provide reasons for your decision.";
        public static readonly string AnalysisRequest = "Analyze the given stock data, financial statements, and market sentiment to determine whether now is a good time to BUY, SELL, or HOLD RCAT. Consider price trends, trading volume, earnings, and recent news sentiment in your response.";
        public static readonly StockAnalysis ResponseStructure = new()
        {
            CurrentPrice = "$2.56",
            NewsSentimentAnalysis = new NewsSentimentAnalysis
            {
                Google = "50% Positive",
                Reddit = "30% Negative",
                Yahoo = "Neutral",
                Finnhub = "60% Positive"
            },
            AiAnalysis = "SELL (85% Confidence)",
            Reasoning = [
                "Stock is in a downward trend.",
                "News sentiment is mixed.",
                "Earnings report on March 17 could increase volatility."
            ]
        };

        public static readonly string InvestmentHorizon = "Short-Term (1-4 weeks)";
    }
}
