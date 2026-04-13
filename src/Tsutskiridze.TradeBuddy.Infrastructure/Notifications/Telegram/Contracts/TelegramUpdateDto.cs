namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Contracts;

public sealed record TelegramUpdateDto(
    long ChatId,
    string? Text,
    string? Command,
    string[] Args
);