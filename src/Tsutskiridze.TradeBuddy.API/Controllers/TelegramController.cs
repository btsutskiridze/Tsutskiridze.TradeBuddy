using Microsoft.AspNetCore.Mvc;
using Tsutskiridze.TradeBuddy.API.Contracts.Telegram;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramController : ControllerBase
    {
        private readonly ITelegramWebhookRouter _router;

        public TelegramController(ITelegramWebhookRouter router)
        {
            _router = router;
        }

        [HttpPost("Webhook")]
        public async Task<IActionResult> Webhook(TelegramWebhookRequest request, CancellationToken ct)
        {
            var updateDto = ExtractTelegramUpdateDto(request);

            var result = await _router.RouteAsync(updateDto, ct);

            return result is null ? Ok() : Ok(result);
        }

        private static TelegramUpdateDto ExtractTelegramUpdateDto(TelegramWebhookRequest request)
        {
            var text = request.Message?.Text;

            string? command = null;
            string[] args = [];

            if (!string.IsNullOrWhiteSpace(text))
            {
                var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                command = parts[0];
                args = parts.Skip(1).ToArray();
            }

            var update = new TelegramUpdateDto(
                request.Message!.Chat.Id,
                text,
                command,
                args
            );
            
            return update;
        }
    }
}