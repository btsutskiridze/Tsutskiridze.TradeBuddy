using System.Text.Json.Serialization;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Contracts;

public class TelegramUpdateResultDto
{
    [JsonPropertyName("method")]
    public string Method = "sendMessage";

    [JsonPropertyName("chat_id")]
    public long ChatId { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("parse_mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ParseMode { get; set; }

    [JsonPropertyName("reply_markup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ReplyMarkup { get; set; }
}