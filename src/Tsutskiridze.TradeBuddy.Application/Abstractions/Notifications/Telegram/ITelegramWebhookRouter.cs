using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;

public interface ITelegramWebhookRouter
{
    Task<TelegramUpdateResultDto?> RouteAsync(TelegramUpdateDto update, CancellationToken ct);
}