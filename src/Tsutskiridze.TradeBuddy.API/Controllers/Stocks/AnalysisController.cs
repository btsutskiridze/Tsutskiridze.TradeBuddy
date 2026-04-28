using Microsoft.AspNetCore.Mvc;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock.Services;

namespace Tsutskiridze.TradeBuddy.API.Controllers.Stocks
{
    [ApiController]
    [Route("api/stocks/{symbol}/analysis")]
    public class AnalysisController : ControllerBase
    {
        private readonly StockAnalysisGenerator _stockAnalysisGenerator;

        public AnalysisController(StockAnalysisGenerator stockAnalysisGenerator)
        {
            _stockAnalysisGenerator = stockAnalysisGenerator;
        }

        [HttpPost]
        public async Task<IActionResult> Analyze([FromRoute] string symbol)
        {
            var result = await _stockAnalysisGenerator.AnalyzeAsync(symbol);

            if (result == null)
            {
                return BadRequest("Stock analysis returned null");
            }

            return Ok(result);
        }
    }
}
