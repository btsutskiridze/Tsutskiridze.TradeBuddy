using Mediator;
using Microsoft.AspNetCore.Mvc;
using Tsutskiridze.TradeBuddy.Application.Common.Enums;
using Tsutskiridze.TradeBuddy.Application.Features.News.Queries;
using Tsutskiridze.TradeBuddy.Application.Features.News.Queries.Finnhub;
using Tsutskiridze.TradeBuddy.Application.Features.News.Queries.Google;
using Tsutskiridze.TradeBuddy.Application.Features.News.Queries.Reddit;
using Tsutskiridze.TradeBuddy.Application.Features.News.Queries.Yahoo;

namespace Tsutskiridze.TradeBuddy.API.Controllers.Stocks
{
    [ApiController]
    [Route("api/stocks/{symbol}/news")]
    public class NewsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NewsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromRoute] string symbol, [FromQuery] int limit = 10)
        {
            var result = await _mediator.Send(new StockNewsQuery(symbol, limit));

            return Ok(result);
        }

        [HttpGet("reddit")]
        public async Task<IActionResult> GetReddit(
            [FromRoute] string symbol,
            [FromQuery] int limit = 10,
            [FromQuery] string sortType = "new")
        {
            var sort = Enum.TryParse<RedditSortType>(sortType, true, out var sortTypeEnum)
                ? sortTypeEnum
                : RedditSortType.New;

            var posts = await _mediator.Send(new RedditNewsQuery(symbol, sort, limit));

            return Ok(posts);
        }

        [HttpGet("google")]
        public async Task<IActionResult> GetGoogle([FromRoute] string symbol, [FromQuery] int limit = 10)
        {
            var news = await _mediator.Send(new GoogleNewsQuery(symbol, limit));

            return Ok(news);
        }

        [HttpGet("yahoo")]
        public async Task<IActionResult> GetYahoo([FromRoute] string symbol, [FromQuery] int limit = 10)
        {
            var news = await _mediator.Send(new YahooNewsQuery(symbol, limit));

            return Ok(news);
        }

        [HttpGet("finnhub")]
        public async Task<IActionResult> GetFinnhub(
            [FromRoute] string symbol,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int limit = 10)
        {
            var news = await _mediator.Send(new FinnhubNewsQuery(symbol, from, to, limit));

            return Ok(news);
        }
    }
}
