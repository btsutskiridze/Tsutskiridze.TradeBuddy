using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.Bloom.Core.Common.Base;

namespace Tsutskiridze.TradeBuddy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramController : ApiControllerBase
    {
        private readonly ITelegramBotClient _botClient;

        public TelegramController(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] Update update)
        {
            if (update == null)
                return BadRequest();

            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                await _botClient.SendMessage(message.Chat.Id, $"You said: {message.Text}");
            }

            return Ok();
        }
    }
}
