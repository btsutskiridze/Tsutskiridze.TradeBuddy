namespace Tsutskiridze.TradeBuddy.Core.Constants
{
    public static class StockPromptParams
    {
        //public static readonly string AnalysisRequest = "Analyze the given stock data and market sentiment to determine if now is a good time to BUY, SELL, or HOLD RCAT. Provide reasons for your decision.";
        public static readonly string AnalysisRequest = "Analyze the given stock data, financial statements, and market sentiment to determine whether now is a good time to BUY, SELL, or HOLD RCAT. Consider price trends, trading volume, earnings, and recent news sentiment in your response.";
        public static readonly string InvestmentHorizon = "Short-Term (1-4 weeks)";
    }
}
