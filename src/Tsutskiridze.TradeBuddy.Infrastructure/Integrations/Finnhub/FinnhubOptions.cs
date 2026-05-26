namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Finnhub
{
    public class FinnhubOptions
    {
        public const string SectionName = "Finnhub";
        public const string ResiliencePipelineName = SectionName;
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}

