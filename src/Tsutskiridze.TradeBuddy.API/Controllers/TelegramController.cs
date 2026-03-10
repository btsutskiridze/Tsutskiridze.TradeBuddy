using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Tsutskiridze.TradeBuddy.API.Contracts.Telegram;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot;

namespace Tsutskiridze.TradeBuddy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramController : ControllerBase
    {
        private readonly TelegramWebhookService _telegramService;
        private readonly ITelegramWebhookRouter _router;

        public TelegramController(ITelegramWebhookRouter router)
        {
            _router = router;
        }

        [HttpPost("Webhook")]
        public async Task<IActionResult> Webhook(TelegramWebhookRequest request, CancellationToken ct)
        {
            var update = ExtractTelegramUpdateDto(request);

            var result = await _router.RouteAsync(update, ct);

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