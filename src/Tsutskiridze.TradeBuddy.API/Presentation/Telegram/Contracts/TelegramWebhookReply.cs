using System.Text.Json.Serialization;
using Telegram.Bot.Types.Enums;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;

public sealed class TelegramWebhookReply
{
    [JsonPropertyName("method")]
    public string Method { get; init; } = "sendMessage";

    [JsonPropertyName("chat_id")]
    public long ChatId { get; init; }

    [JsonPropertyName("text")]
    public required string TextMessage { get; init; }

    [JsonPropertyName("parse_mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ParseMode { get; init; }

    [JsonPropertyName("reply_markup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ReplyMarkup { get; init; }

    public static TelegramWebhookReply Text(long chatId, string text, ParseMode? parseMode = null) =>
        new()
        {
            ChatId = chatId,
            TextMessage = text,
            ParseMode = parseMode?.ToString()
        };
}
