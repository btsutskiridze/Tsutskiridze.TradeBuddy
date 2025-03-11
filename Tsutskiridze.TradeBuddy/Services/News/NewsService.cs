using Tsutskiridze.TradeBuddy.DTOs;
using Tsutskiridze.TradeBuddy.DTOs.Finnhub;
using Tsutskiridze.TradeBuddy.DTOs.Google;
using Tsutskiridze.TradeBuddy.DTOs.Reddit;
using Tsutskiridze.TradeBuddy.DTOs.Yahoo;

namespace Tsutskiridze.TradeBuddy.Services.News
{
    public class NewsService
    {
        private readonly FinnhubService _finnhub;
        private readonly RedditService _reddit;
        private readonly YahooSraper _yahooSraper;
        private readonly GoogleScraper _googleScraper;

        public NewsService(FinnhubService finnhub, RedditService reddit, YahooSraper yahooSraper, GoogleScraper googleScraper)
        {
            _finnhub = finnhub;
            _reddit = reddit;
            _yahooSraper = yahooSraper;
            _googleScraper = googleScraper;
        }

        public async Task<AllNews?> GetAllNews(string symbol, int? limit = null)
        {
            var googleTask = GetGoogleNewsAsync(symbol, limit);
            var redditTask = GetRedditNewsAsync(symbol, "new", limit);
            var yahooTask = GetYahooNewsAsync(symbol, limit);
            var finnhubTask = GetFinnhubNewsAsync(symbol, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, limit);

            // Wait until all tasks complete.
            await Task.WhenAll(googleTask, redditTask, yahooTask, finnhubTask);

            // Check if all results are null.
            if (googleTask.Result == null && redditTask.Result == null &&
                yahooTask.Result == null && finnhubTask.Result == null)
            {
                return null;
            }

            // Fill the AllNews DTO with the results.
            return new AllNews
            {
                Google = googleTask.Result,
                Reddit = redditTask.Result,
                Yahoo = yahooTask.Result,
                Finnhub = finnhubTask.Result
            };
        }


        public async Task<List<GoogleNews>?> GetGoogleNewsAsync(string symbol, int? limit = null)
        {
            return await _googleScraper.GetNewsAsync(symbol, limit);
        }

        public async Task<List<RedditPost>?> GetRedditNewsAsync(string symbol, string sort, int? limit = null)
        {
            return await _reddit.GetTopPostsAsync(symbol, sort, limit);
        }

        public async Task<List<YahooNews>?> GetYahooNewsAsync(string symbol, int? limit = null)
        {
            return await _yahooSraper.GetNewsAsync(symbol, limit);
        }

        public async Task<List<FinnhubNews>?> GetFinnhubNewsAsync(string symbol, DateTime from, DateTime to, int? limit = null)
        {
            return await _finnhub.GetCompanyNewsAsync(symbol, from, to, limit);
        }
    }
}
