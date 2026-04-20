using Tsutskiridze.TradeBuddy.API.Contracts.Telegram;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Abstractions;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram;

public class TelegramCommandParser:ITelegramCommandParser
{
    public TelegramCommandDispatchRequest? Parse(TelegramWebhookRequest request)
    {
        var text = request.Message?.Text?.Trim();

        if (string.IsNullOrWhiteSpace(text))
            return null;

        if (!text.StartsWith('/'))
            return null;

        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].Split('@')[0].ToLowerInvariant();
        var args = parts.Skip(1).ToArray();

        return new TelegramCommandDispatchRequest(
            ChatId: request.Message!.Chat.Id,
            RawText: text,
            Command: command,
            Args: args);
    }
}