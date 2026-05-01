using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.API.Contracts.Telegram;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram;

namespace Tsutskiridze.TradeBuddy.API.Telegram.Contracts;

public sealed class TelegramCommandDispatchResponse
{
    public TelegramWebhookResponse? WebhookReply { get; init; }

    public IReadOnlyList<TelegramOutgoingMessage> PriorMessages { get; init; } =
        Array.Empty<TelegramOutgoingMessage>();

    public bool HasWebhookReply => WebhookReply is not null;
    public bool HasPriorMessages => PriorMessages.Count > 0;

    public static TelegramCommandDispatchResponse Empty() =>
        new();

    public static TelegramCommandDispatchResponse ReplyOnly(TelegramWebhookResponse reply) =>
        new()
        {
            WebhookReply = reply
        };

    public static TelegramCommandDispatchResponse PriorOnly(IReadOnlyList<TelegramOutgoingMessage> priorMessages) =>
        new()
        {
            PriorMessages = priorMessages
        };

    public static TelegramCommandDispatchResponse ReplyWithPrior(
        TelegramWebhookResponse reply,
        IReadOnlyList<TelegramOutgoingMessage> priorMessages) =>
        new()
        {
            WebhookReply = reply,
            PriorMessages = priorMessages
        };

    public static TelegramCommandDispatchResponse TextReply(
        long chatId,
        string text,
        ParseMode? parseMode = null,
        IReadOnlyList<TelegramOutgoingMessage>? priorMessages = null) =>
        new()
        {
            WebhookReply = TelegramWebhookResponse.Text(chatId, text, parseMode),
            PriorMessages = priorMessages ?? Array.Empty<TelegramOutgoingMessage>()
        };
}