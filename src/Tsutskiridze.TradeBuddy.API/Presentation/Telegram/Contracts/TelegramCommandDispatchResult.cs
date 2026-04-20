using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;

public sealed class TelegramCommandDispatchResult
{
    public TelegramWebhookReply? WebhookReply { get; init; }

    public IReadOnlyList<TelegramOutgoingMessage> PriorMessages { get; init; } =
        Array.Empty<TelegramOutgoingMessage>();

    public bool HasWebhookReply => WebhookReply is not null;
    public bool HasPriorMessages => PriorMessages.Count > 0;

    public static TelegramCommandDispatchResult Empty() =>
        new();

    public static TelegramCommandDispatchResult ReplyOnly(TelegramWebhookReply reply) =>
        new()
        {
            WebhookReply = reply
        };

    public static TelegramCommandDispatchResult PriorOnly(IReadOnlyList<TelegramOutgoingMessage> priorMessages) =>
        new()
        {
            PriorMessages = priorMessages
        };

    public static TelegramCommandDispatchResult ReplyWithPrior(
        TelegramWebhookReply reply,
        IReadOnlyList<TelegramOutgoingMessage> priorMessages) =>
        new()
        {
            WebhookReply = reply,
            PriorMessages = priorMessages
        };

    public static TelegramCommandDispatchResult TextReply(
        long chatId,
        string text,
        ParseMode? parseMode = null,
        IReadOnlyList<TelegramOutgoingMessage>? priorMessages = null) =>
        new()
        {
            WebhookReply = TelegramWebhookReply.Text(chatId, text, parseMode),
            PriorMessages = priorMessages ?? Array.Empty<TelegramOutgoingMessage>()
        };
}
