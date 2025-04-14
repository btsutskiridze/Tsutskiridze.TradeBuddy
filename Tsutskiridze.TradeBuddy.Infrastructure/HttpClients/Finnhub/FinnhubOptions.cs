namespace Tsutskiridze.TradeBuddy.Infrastructure.HttpClients.Finnhub
{
    public class FinnhubOptions
    {
        public const string SectionName = "Finnhub";
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}
