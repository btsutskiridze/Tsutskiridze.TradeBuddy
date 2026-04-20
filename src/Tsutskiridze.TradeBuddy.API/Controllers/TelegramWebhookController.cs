using Microsoft.AspNetCore.Mvc;
using Tsutskiridze.TradeBuddy.API.Contracts.Telegram;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Abstractions;

namespace Tsutskiridze.TradeBuddy.API.Controllers
{
    [ApiController]
    [Route("api/telegram")]
    public class TelegramWebhookController : ControllerBase
    {
        private readonly ITelegramCommandParser _parser;
        private readonly ITelegramCommandDispatcher _dispatcher;

        public TelegramWebhookController(ITelegramCommandParser parser, ITelegramCommandDispatcher dispatcher)
        {
            _parser = parser;
            _dispatcher = dispatcher;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] TelegramWebhookRequest request, CancellationToken ct)
        {
            var command = _parser.Parse(request);
            if (command == null)
                return Ok();
            
            var result = await _dispatcher.Dispatch(command, ct);

            return result.HasWebhookReply ? Ok(result.WebhookReply) : Ok();
        }
    }
}