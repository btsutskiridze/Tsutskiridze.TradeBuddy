namespace Tsutskiridze.TradeBuddy.Application.Contracts.News
{
    public class FinnhubNewsItemDto
    {
        public string Category { get; set; }
        public string Title { get; set; }
        public string Source { get; set; }
        public string Summary { get; set; }
        public string Url { get; set; }
        public string CreateTime { get; set; }
    }
}
