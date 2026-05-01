using System.Text.Json.Serialization;
using Telegram.Bot.Types.Enums;

namespace Tsutskiridze.TradeBuddy.API.Contracts.Telegram;

public sealed class TelegramWebhookResponse
{
    [JsonPropertyName("method")] public string Method = "sendMessage";

    [JsonPropertyName("chat_id")]
    public long ChatId { get; set; }

    [JsonPropertyName("text")]
    public string TextMessage { get; set; }

    [JsonPropertyName("parse_mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ParseMode { get; set; }

    [JsonPropertyName("reply_markup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ReplyMarkup { get; set; }
    
    public static TelegramWebhookResponse Text(long chatId, string text, ParseMode? parseMode = null) =>
        new()
        {
            ChatId = chatId,
            TextMessage = text,
            ParseMode = parseMode?.ToString()
        };
}