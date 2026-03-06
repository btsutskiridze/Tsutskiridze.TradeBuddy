namespace Tsutskiridze.TradeBuddy.Application.Dtos
{
    public class AllNews
    {
        public List<GoogleNews>? Google { get; set; }
        public List<RedditPost>? Reddit { get; set; }
        public List<YahooNews>? Yahoo { get; set; }
        public List<FinnhubNews>? Finnhub { get; set; }
    }
}
