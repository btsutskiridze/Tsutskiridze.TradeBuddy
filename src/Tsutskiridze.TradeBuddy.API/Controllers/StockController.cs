using Microsoft.AspNetCore.Mvc;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis;

namespace Tsutskiridze.TradeBuddy.API.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly StockAnalysisService _stockAnalysis;

        public StockController(StockAnalysisService stockAnalysisService)
        {
            _stockAnalysis = stockAnalysisService;
        }
        
        [HttpPost("{stock}/Analysis")]
        public async Task<IActionResult> AnalyseStock(string stock)
        {
            var result = await _stockAnalysis.ExecuteStockAnalysis(stock);

            if (result == null)
            {
                return BadRequest("Stock analysis returned null");
            }

            return Ok(result);
        }
    }
}
