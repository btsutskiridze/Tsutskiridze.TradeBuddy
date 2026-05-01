using Microsoft.AspNetCore.Mvc;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;

namespace Tsutskiridze.TradeBuddy.API.Controllers
{
    [ApiController]
    [Route("api/yahoo-scraper")]
    public class YahooScraperController : ControllerBase
    {
        private readonly IMarketDataProvider _marketDataProvider;
        private readonly IEmaAdxAtrEvaluationCandleBuilder _emaAdxAtrEvaluationCandleBuilder;

        public YahooScraperController(
            IMarketDataProvider marketDataProvider,
            IEmaAdxAtrEvaluationCandleBuilder emaAdxAtrEvaluationCandleBuilder)
        {
            _marketDataProvider = marketDataProvider;
            _emaAdxAtrEvaluationCandleBuilder = emaAdxAtrEvaluationCandleBuilder;
        }

        [HttpGet("{symbol}/exists")]
        public async Task<IActionResult> StockSymbolExists(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Stock symbol is required.");
            }

            var exists = await _marketDataProvider.StockSymbolExists(symbol);
            return Ok(exists);
        }

        [HttpGet("{symbol}/history")]
        public async Task<IActionResult> GetStockPrevDaysClosePrices(string symbol, [FromQuery] int? days = null)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Stock symbol is required.");
            }

            if (days is <= 0)
            {
                return BadRequest("Days must be greater than 0 when provided.");
            }

            var history = await _marketDataProvider.GetStockPrevDaysClosePrices(symbol, days);
            return Ok(history);
        }

        [HttpGet("{symbol}/overview")]
        public async Task<IActionResult> GetStockOverview(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Stock symbol is required.");
            }

            var overview = await _marketDataProvider.GetStockOverview(symbol);
            return Ok(overview);
        }

        [HttpGet("{symbol}/annual-report")]
        public async Task<IActionResult> GetStockLastAnnualReport(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Stock symbol is required.");
            }

            var annualReport = await _marketDataProvider.GetStockLastAnnualReport(symbol);
            return Ok(annualReport);
        }

        [HttpGet("{symbol}/quote")]
        public async Task<IActionResult> GetStockQuote(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Stock symbol is required.");
            }

            var quote = await _marketDataProvider.GetStockQuote(symbol);
            return Ok(quote);
        }

        [HttpGet("{symbol}/daily-candles")]
        public async Task<IActionResult> GetDailyCandles(
            string symbol,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Stock symbol is required.");
            }

            if (from > to)
            {
                return BadRequest("'from' date cannot be greater than 'to' date.");
            }

            var candles = await _marketDataProvider.GetDailyCandles(symbol, from, to, ct);
            return Ok(candles);
        }

        [HttpGet("{symbol}/daily-strategy-candles")]
        public async Task<IActionResult> GetDailyStrategyCandles(
            string symbol,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Stock symbol is required.");
            }

            if (from > to)
            {
                return BadRequest("'from' date cannot be greater than 'to' date.");
            }

            var candles = await _marketDataProvider.GetDailyCandles(symbol, from, to, ct);
            var strategyCandles = _emaAdxAtrEvaluationCandleBuilder.BuildDailyStrategyCandles(10, 20, 14, 14, candles);

            return Ok(strategyCandles);
        }
    }
}