namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;

public sealed record TelegramCommandDispatchRequest(
    long ChatId,
    string RawText,
    string Command,
    IReadOnlyList<string> Args
);