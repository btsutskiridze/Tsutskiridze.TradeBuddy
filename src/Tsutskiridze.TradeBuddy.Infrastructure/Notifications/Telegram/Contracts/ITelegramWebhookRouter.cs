namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Contracts;

public interface ITelegramWebhookRouter
{
    Task<TelegramUpdateResultDto?> RouteAsync(TelegramUpdateDto update, CancellationToken ct);
}