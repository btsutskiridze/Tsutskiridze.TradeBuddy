namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.AlphaVantage
{
    public class AlphaVantageOptions
    {
        public const string SectionName = "AlphaVantage";
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }
}
