using Mediator;
using Microsoft.AspNetCore.Mvc;
using Tsutskiridze.TradeBuddy.Application.Enums;
using Tsutskiridze.TradeBuddy.Application.Features.News.Queries;
using Tsutskiridze.TradeBuddy.Application.Features.News.Queries.Finnhub;
using Tsutskiridze.TradeBuddy.Application.Features.News.Queries.Google;
using Tsutskiridze.TradeBuddy.Application.Features.News.Queries.Reddit;
using Tsutskiridze.TradeBuddy.Application.Features.News.Queries.Yahoo;


namespace Tsutskiridze.TradeBuddy.API.Controllers
{
    [ApiController]
    [Route("api/news")]
    public class StockNewsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StockNewsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{symbol}")]
        public async Task<IActionResult> GetStockNews(string symbol, [FromQuery] int limit = 10,
            [FromQuery] string sortType = "new")
        {
            var result = await _mediator.Send(new StockNewsQuery(symbol, limit));

            return Ok(result);
        }

        // Reverse the uris for concrete sources:
        [HttpGet("{symbol}/reddit")]
        public async Task<IActionResult> GetRedditPosts(string symbol, [FromQuery] int limit = 10,
            [FromQuery] string sortType = "new")
        {
            RedditSortType sort = Enum.TryParse<RedditSortType>(sortType, true, out var sortTypeEnum)
                ? sortTypeEnum
                : RedditSortType.New;

            var posts = await _mediator.Send(new RedditNewsQuery(symbol, sort, limit));

            return Ok(posts);
        }

        [HttpGet("{symbol}/google")]
        public async Task<IActionResult> GetGoogleNews(string symbol, [FromQuery] int limit = 10)
        {
            var news = await _mediator.Send(new GoogleNewsQuery(symbol, limit));

            return Ok(news);
        }

        [HttpGet("{symbol}/yahoo")]
        public async Task<IActionResult> GetYahooNews(string symbol, [FromQuery] int limit = 10)
        {
            var news = await _mediator.Send(new YahooNewsQuery(symbol, limit));

            return Ok(news);
        }

        [HttpGet("{symbol}/finnhub")]
        public async Task<IActionResult> GetFinnhubNews(
            string symbol,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int limit = 10
        )
        {
            var news = await _mediator.Send(new FinnhubNewsQuery(symbol, from, to, limit));

            return Ok(news);
        }
    }
}