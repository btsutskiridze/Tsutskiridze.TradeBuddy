namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.FinancialModelingPrep
{
    public class FinancialModelingPrepOptions
    {
        public const string SectionName = "Fmp";
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }
}
