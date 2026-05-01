using Mediator;
using Microsoft.AspNetCore.Mvc;
using Tsutskiridze.TradeBuddy.API.Contracts.Stocks;
using Tsutskiridze.TradeBuddy.Application.Common.Enums;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Queries.GetStockNews;

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
        public async Task<IActionResult> Get(
            [FromRoute] string symbol,
            [FromQuery] GetStockNewsRequest request,
            CancellationToken ct
        )
        {
            var result = await _mediator.Send(
                new GetStockNewsQuery(symbol, Limit: request.Limit),
                ct
            );

            return Ok(result);
        }

        [HttpGet("reddit")]
        public async Task<IActionResult> GetReddit(
            [FromRoute] string symbol,
            [FromQuery] GetRedditStockNewsRequest request,
            CancellationToken ct
        )
        {
            var result = await _mediator.Send(
                new GetStockNewsQuery(
                    symbol,
                    [NewsSource.Reddit],
                    request.Limit,
                    SortType: request.ToSortType()),
                ct
            );

            return Ok(result);
        }

        [HttpGet("google")]
        public async Task<IActionResult> GetGoogle(
            [FromRoute] string symbol,
            [FromQuery] GetStockNewsRequest request,
            CancellationToken ct
        )
        {
            var result = await _mediator.Send(
                new GetStockNewsQuery(symbol, [NewsSource.Google], request.Limit),
                ct
            );

            return Ok(result);
        }

        [HttpGet("yahoo")]
        public async Task<IActionResult> GetYahoo(
            [FromRoute] string symbol,
            [FromQuery] GetStockNewsRequest request,
            CancellationToken ct
        )
        {
            var result = await _mediator.Send(
                new GetStockNewsQuery(symbol, [NewsSource.Yahoo], request.Limit),
                ct
            );

            return Ok(result);
        }

        [HttpGet("finnhub")]
        public async Task<IActionResult> GetFinnhub(
            [FromRoute] string symbol,
            [FromQuery] GetFinnhubStockNewsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetStockNewsQuery(
                    symbol,
                    [NewsSource.Finnhub],
                    request.Limit,
                    request.From,
                    request.To),
                cancellationToken);

            return Ok(result);
        }
    }
}