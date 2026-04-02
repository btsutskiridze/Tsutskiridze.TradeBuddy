using Microsoft.AspNetCore.Mvc;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;

namespace Tsutskiridze.TradeBuddy.API.Controllers
{
    [ApiController]
    [Route("api/yahoo-scraper")]
    public class YahooScraperController : ControllerBase
    {
        private readonly IYahooMarketDataProvider _yahooMarketDataProvider;

        public YahooScraperController(IYahooMarketDataProvider yahooMarketDataProvider)
        {
            _yahooMarketDataProvider = yahooMarketDataProvider;
        }

        [HttpGet("{symbol}/exists")]
        public async Task<IActionResult> StockSymbolExists(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Stock symbol is required.");
            }

            var exists = await _yahooMarketDataProvider.StockSymbolExists(symbol);
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

            var history = await _yahooMarketDataProvider.GetStockPrevDaysClosePrices(symbol, days);
            return Ok(history);
        }

        [HttpGet("{symbol}/overview")]
        public async Task<IActionResult> GetStockOverview(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Stock symbol is required.");
            }

            var overview = await _yahooMarketDataProvider.GetStockOverview(symbol);
            return Ok(overview);
        }

        [HttpGet("{symbol}/annual-report")]
        public async Task<IActionResult> GetStockLastAnnualReport(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Stock symbol is required.");
            }

            var annualReport = await _yahooMarketDataProvider.GetStockLastAnnualReport(symbol);
            return Ok(annualReport);
        }

        [HttpGet("{symbol}/quote")]
        public async Task<IActionResult> GetStockQuote(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Stock symbol is required.");
            }

            var quote = await _yahooMarketDataProvider.GetStockQuote(symbol);
            return Ok(quote);
        }
    }
}
