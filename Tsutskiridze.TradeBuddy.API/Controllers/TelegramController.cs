using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Telegram.Bot.Types;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot;

namespace Tsutskiridze.TradeBuddy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramController : ControllerBase
    {
        private readonly TelegramWebhookService _telegramService;
        private readonly ILogger<TelegramController> _logger;

        public TelegramController(TelegramWebhookService handler, ILogger<TelegramController> logger)
        {
            _telegramService = handler;
            _logger = logger;
        }

        [HttpPost("Webhook")]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();
            _logger.LogInformation("Received JSON: {json}", json);
            reader.Close();

            var options = new JsonSerializerSettings
            {
                Converters = {
                    new UnixDateTimeConverter(),
                    new StringEnumConverter(
                        new Newtonsoft.Json.Serialization.SnakeCaseNamingStrategy
                        {
                            ProcessDictionaryKeys = true,
                            OverrideSpecifiedNames = true
                        },
                        allowIntegerValues: false
                    )
                },
                Error = (sender, args) =>
                {
                    _logger.LogError(args.ErrorContext.Error, "Error during deserialization");
                    args.ErrorContext.Handled = true;
                },
            };

            var update = JsonConvert.DeserializeObject<Update>(json, options);

            if (update == null)
            {
                _logger.LogError("Deserialization failed - update is null");
                return BadRequest();
            }

            await _telegramService.HandleUpdate(update);

            return Ok();
        }
    }
}
