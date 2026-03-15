namespace Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;

public interface ITelegramSender
{
    Task SendMessage(long chatId, string text, CancellationToken ct = default);
    Task SendTypingAction(long chatId, CancellationToken ct = default);
}