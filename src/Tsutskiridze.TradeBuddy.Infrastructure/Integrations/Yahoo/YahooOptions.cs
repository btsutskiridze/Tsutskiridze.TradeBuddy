namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo
{
    public class YahooOptions
    {
        public const string SectionName = "Yahoo";
        public const string PageResiliencePipelineName = SectionName + ":Page";
        public const string HistoryResiliencePipelineName = SectionName + ":History";
        public const string StockQuoteResiliencePipelineName = SectionName + ":StockQuote";
        public string BaseUrl { get; set; } = string.Empty;
        public string Query2ApiUrl { get; set; } = string.Empty;
    }
}

