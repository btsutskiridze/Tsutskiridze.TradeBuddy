namespace Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock.Prompts
{
    public static class StockAnalysisPromptTemplate
    {
        public static readonly string AnalysisRequest =
            "Analyze the supplied stock quote, trading history, " +
            "financials and recent Google/Yahoo/Reddit headlines for Stock " +
            "over a 1-4 week horizon. Compare price to its 50-day average " +
            "and 52-week high, today’s volume to the 50-day average, and " +
            "review earnings and ROE. From these factors and headline " +
            "sentiment, decide whether Stock is a BUY, SELL or HOLD and " +
            "briefly list the key reasons.";

        public static readonly string InvestmentHorizon = "Short-Term (1-4 weeks)";
    }
}
