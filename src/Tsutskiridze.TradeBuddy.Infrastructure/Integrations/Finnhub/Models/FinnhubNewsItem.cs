namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Finnhub.Models
{
    public class FinnhubNewsItem
    {
        public string Category { get; set; }
        public string Title { get; set; }
        public string Source { get; set; }
        public string Summary { get; set; }
        public string Url { get; set; }
        public string CreateTime { get; set; }
    }
}
