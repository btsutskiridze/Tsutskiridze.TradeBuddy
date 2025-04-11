using Tsutskiridze.TradeBuddy.DTOs;
using Tsutskiridze.TradeBuddy.DTOs.Finnhub;
using Tsutskiridze.TradeBuddy.DTOs.Google;
using Tsutskiridze.TradeBuddy.DTOs.Reddit;
using Tsutskiridze.TradeBuddy.DTOs.Yahoo;
using Tsutskiridze.TradeBuddy.Helpers;
using Tsutskiridze.TradeBuddy.Services.Yahoo;

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
            var googleTask = GetGoogleNews(symbol, limit);
            var redditTask = GetRedditNews(symbol, RedditSortType.New, limit);
            var yahooTask = GetYahooNews(symbol, limit);
            var finnhubTask = GetFinnhubNews(symbol, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, limit);

            await Task.WhenAll(googleTask, redditTask, yahooTask, finnhubTask);

            if (googleTask.Result == null && redditTask.Result == null &&
                yahooTask.Result == null && finnhubTask.Result == null)
            {
                throw new Exception("Failed to get news");
            }

            return new AllNews
            {
                Google = googleTask.Result,
                Reddit = redditTask.Result,
                Yahoo = yahooTask.Result,
                Finnhub = finnhubTask.Result
            };
        }


        public async Task<List<GoogleNews>?> GetGoogleNews(string symbol, int? limit = null)
        {
            return await _googleScraper.GetNewsAsync(symbol, limit);
        }

        public async Task<List<RedditPost>?> GetRedditNews(string symbol, RedditSortType sort, int? limit = null)
        {
            return await _reddit.GetRedditPosts(symbol, sort, limit);
        }

        public async Task<List<YahooNews>?> GetYahooNews(string symbol, int? limit = null)
        {
            return await _yahooSraper.GetNewsAsync(symbol, limit);
        }

        public async Task<List<FinnhubNews>?> GetFinnhubNews(string symbol, DateTime from, DateTime to, int? limit = null)
        {
            return await _finnhub.GetCompanyNewsAsync(symbol, from, to, limit);
        }
    }
}
