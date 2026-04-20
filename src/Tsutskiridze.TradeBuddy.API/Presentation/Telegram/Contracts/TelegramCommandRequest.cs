namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;

public sealed record TelegramCommandRequest(
    long ChatId,
    string RawText,
    string Command,
    IReadOnlyList<string> Args
);