namespace Tsutskiridze.TradeBuddy.Application.Contracts.News
{
    public class StockNewsDto
    {
        public List<GoogleNewsItemDto>? Google { get; set; }
        public List<RedditPostDto>? Reddit { get; set; }
        public List<YahooNewsItemDto>? Yahoo { get; set; }
        public List<FinnhubNewsItemDto>? Finnhub { get; set; }
    }
}
