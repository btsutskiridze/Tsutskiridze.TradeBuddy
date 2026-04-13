using Microsoft.AspNetCore.Mvc;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis.Services;

namespace Tsutskiridze.TradeBuddy.API.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly StockAnalysisGenerator _stockAnalysis;

        public StockController(StockAnalysisGenerator stockAnalysisGenerator)
        {
            _stockAnalysis = stockAnalysisGenerator;
        }
        
        [HttpPost("{stock}/Analysis")]
        public async Task<IActionResult> AnalyseStock(string stock)
        {
            var result = await _stockAnalysis.AnalyzeAsync(stock);

            if (result == null)
            {
                return BadRequest("Stock analysis returned null");
            }

            return Ok(result);
        }
    }
}
