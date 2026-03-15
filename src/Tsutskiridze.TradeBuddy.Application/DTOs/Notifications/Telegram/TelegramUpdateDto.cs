namespace Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;

public sealed record TelegramUpdateDto(
    long ChatId,
    string? Text,
    string? Command,
    string[] Args
);