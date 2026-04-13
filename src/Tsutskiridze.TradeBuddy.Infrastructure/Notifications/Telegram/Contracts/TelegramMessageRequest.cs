namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Contracts;

public sealed record TelegramMessageRequest(
    long ChatId,
    string? Text,
    string? Command,
    string[] Args
);