namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;

public interface ITelegramSender
{
    Task Send(TelegramOutgoingMessage message, CancellationToken ct = default);
    Task Send(IEnumerable<TelegramOutgoingMessage> messages, CancellationToken ct = default);
    Task SendTypingAction(long chatId, CancellationToken ct = default);
}