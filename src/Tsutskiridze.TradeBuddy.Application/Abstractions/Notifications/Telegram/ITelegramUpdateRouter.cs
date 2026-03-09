using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;

public interface ITelegramUpdateRouter
{
    Task RouteAsync(TelegramUpdateDto updateDto, CancellationToken ct);
}