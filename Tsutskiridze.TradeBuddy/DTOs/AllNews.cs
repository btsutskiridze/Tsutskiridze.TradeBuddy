using Tsutskiridze.TradeBuddy.DTOs.Finnhub;
using Tsutskiridze.TradeBuddy.DTOs.Google;
using Tsutskiridze.TradeBuddy.DTOs.Reddit;
using Tsutskiridze.TradeBuddy.DTOs.Yahoo;

namespace Tsutskiridze.TradeBuddy.DTOs
{
    public class AllNews
    {
        public List<GoogleNews>? Google { get; set; }
        public List<RedditPost>? Reddit { get; set; }
        public List<YahooNews>? Yahoo { get; set; }
        public List<FinnhubNews>? Finnhub { get; set; }
    }
}
