using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        private readonly ILogger<TelegramController> _logger;

        public TelegramController(ITelegramBotClient botClient, ILogger<TelegramController> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        [HttpPost("Webhook")]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();
            _logger.LogInformation("Received JSON: {json}", json);
            reader.Close();

            var options = new Newtonsoft.Json.JsonSerializerSettings
            {
                Converters = { new UnixDateTimeConverter() },
                Error = (sender, args) =>
                {
                    _logger.LogError(args.ErrorContext.Error, "Error during deserialization");
                    args.ErrorContext.Handled = true;
                }
            };

            var update = JsonConvert.DeserializeObject<Update>(json, options);

            if (update == null)
            {
                _logger.LogError("Deserialization failed - update is null");
                return BadRequest();
            }

            try
            {
                if (update.Type == UpdateType.Message)
                {
                    var message = update.Message;
                    _logger.LogInformation("Processing message update");
                    await _botClient.SendMessage(message.Chat.Id, $"You said: {message.Text}");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Processing failed");
                return BadRequest();
            }

        }
    }
}
