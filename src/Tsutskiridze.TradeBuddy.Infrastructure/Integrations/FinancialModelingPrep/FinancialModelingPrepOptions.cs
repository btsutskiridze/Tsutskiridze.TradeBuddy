namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep
{
    public class FinancialModelingPrepOptions
    {
        public const string SectionName = "Fmp";
        public const string ResiliencePipelineName = SectionName;
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }
}

