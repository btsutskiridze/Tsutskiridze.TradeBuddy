using Microsoft.AspNetCore.Mvc;
using System.Net;
using Tsutskiridze.Bloom.Core.Common.Base;
using Tsutskiridze.TradeBuddy.Services;

namespace Tsutskiridze.TradeBuddy.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ApiControllerBase
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
                return JsonResult("Stock analysis returned null", HttpStatusCode.InternalServerError);
            }

            return JsonResult(result);
        }
    }
}
