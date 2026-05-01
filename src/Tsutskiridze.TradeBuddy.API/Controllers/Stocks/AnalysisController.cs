using Mediator;
using Microsoft.AspNetCore.Mvc;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock;

namespace Tsutskiridze.TradeBuddy.API.Controllers.Stocks
{
    [ApiController]
    [Route("api/stocks/{symbol}/analysis")]
    public class AnalysisController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnalysisController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Analyze([FromRoute] string symbol, [FromQuery] long chatId)
        {
            var result = await _mediator.Send(new AnalyseStockCommand(chatId, symbol));
            
            return Ok(result);
        }
    }
}