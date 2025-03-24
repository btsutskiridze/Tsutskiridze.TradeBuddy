using Microsoft.AspNetCore.Mvc;
using Tsutskiridze.Bloom.Core.Common.Base;
using Tsutskiridze.TradeBuddy.Helpers;
using Tsutskiridze.TradeBuddy.Services.News;


namespace Tsutskiridze.TradeBuddy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ApiControllerBase
    {

        private readonly NewsService _news;
        private readonly ILogger<NewsController> _logger;

        public NewsController(NewsService news, ILogger<NewsController> logger)
        {
            _news = news;
            _logger = logger;
        }


        [HttpGet("reddit/{symbol}")]
        public async Task<IActionResult> GetRedditPosts(string symbol, [FromQuery] int limit = 10, [FromQuery] string sortType = "new")
        {
            RedditSortType sort = Enum.TryParse<RedditSortType>(sortType, true, out var sortTypeEnum) ? sortTypeEnum : RedditSortType.New;

            var posts = await _news.GetRedditNews(symbol, sort, limit);

            return JsonResult(posts);
        }

        [HttpGet("google/{symbol}")]
        public async Task<IActionResult> GetGoogleNews(string symbol, [FromQuery] int limit = 10)
        {
            var news = await _news.GetGoogleNews(symbol, limit);

            return JsonResult(news);
        }

        [HttpGet("yahoo/{symbol}")]
        public async Task<IActionResult> GetYahooNews(string symbol = "NVDA", [FromQuery] int limit = 10)
        {
            var news = await _news.GetYahooNews(symbol, limit);

            return JsonResult(news);
        }

        [HttpGet("finnhub/{symbol}")]
        public async Task<IActionResult> GetFinnhubNews(
            string symbol,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int limit = 10
        )
        {
            from = from == DateTime.MinValue ? DateTime.UtcNow.AddDays(-7) : from;
            to = to == DateTime.MinValue ? DateTime.UtcNow : to;

            var news = await _news.GetFinnhubNews(symbol, from, to, limit);

            return JsonResult(news);
        }
    }
}
