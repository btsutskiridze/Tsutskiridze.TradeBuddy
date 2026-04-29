namespace Tsutskiridze.TradeBuddy.API.Telegram.Contracts;

public sealed record TelegramCommandDispatchRequest(
    long ChatId,
    string RawText,
    string Command,
    IReadOnlyList<string> Args
);