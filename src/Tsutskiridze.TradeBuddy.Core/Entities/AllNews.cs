using Tsutskiridze.TradeBuddy.Application.Dtos.Finnhub;
using Tsutskiridze.TradeBuddy.Application.Dtos.Google;
using Tsutskiridze.TradeBuddy.Application.Dtos.Reddit;
using Tsutskiridze.TradeBuddy.Application.Dtos.Yahoo;

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
