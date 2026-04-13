namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Contracts;

public interface ITelegramWebhookRouter
{
    Task<TelegramMessageResponse?> RouteAsync(TelegramMessageRequest update, CancellationToken ct);
}