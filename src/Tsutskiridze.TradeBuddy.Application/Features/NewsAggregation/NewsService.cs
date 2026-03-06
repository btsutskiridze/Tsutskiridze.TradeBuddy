using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.Contracts.News;
using Tsutskiridze.TradeBuddy.Application.Enums;
using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Features.NewsAggregation
{
    public class NewsService
    {
        private readonly IFinnhubNewsProvider _finnhub;
        private readonly IRedditNewsProvider _reddit;
        private readonly IYahooNewsProvider _yahooSraper;
        private readonly IGoogleNewsProvider _googleScraper;

        public NewsService(IFinnhubNewsProvider finnhub,
            IRedditNewsProvider reddit,
            IYahooNewsProvider yahooSraper,
            IGoogleNewsProvider googleScraper)
        {
            _finnhub = finnhub;
            _reddit = reddit;
            _yahooSraper = yahooSraper;
            _googleScraper = googleScraper;
        }

        public async Task<StockNewsDto?> GetAllNews(string symbol, int? limit = null)
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

            return new StockNewsDto
            {
                Google = googleTask.Result,
                Reddit = redditTask.Result,
                Yahoo = yahooTask.Result,
                Finnhub = finnhubTask.Result
            };
        }

        public async Task<List<GoogleNewsItemDto>?> GetGoogleNews(string symbol, int? limit = null)
        {
            return await _googleScraper.GetNewsAsync(symbol, limit);
        }

        public async Task<List<RedditPostDto>?> GetRedditNews(string symbol, RedditSortType sort, int? limit = null)
        {
            return await _reddit.GetRedditPosts(symbol, sort, limit);
        }

        public async Task<List<YahooNewsItemDto>?> GetYahooNews(string symbol, int? limit = null)
        {
            return await _yahooSraper.GetNewsAsync(symbol, limit);
        }

        public async Task<List<FinnhubNewsItemDto>?> GetFinnhubNews(string symbol, DateTime from, DateTime to, int? limit = null)
        {
            return await _finnhub.GetCompanyNewsAsync(symbol, from, to, limit);
        }
    }
}
