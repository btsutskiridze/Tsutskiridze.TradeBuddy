using System.Text.Json.Serialization;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;

public sealed class TelegramCommandResponse
{
    public WebhookResponse Response { get; init; } = new();
    public IReadOnlyList<TelegramOutgoingMessage> PriorMessages { get; init; } = [];
    public static TelegramCommandResponse Create(
        long chatId, 
        string text, 
        ParseMode? parseMode = null, 
        IReadOnlyList<TelegramOutgoingMessage>? priorMessages = null
    ) =>
        new()
        {
            Response = new WebhookResponse()
            {
                ChatId = chatId, 
                Text = text, 
                ParseMode = parseMode?.ToString()
            },
            PriorMessages = priorMessages ?? Array.Empty<TelegramOutgoingMessage>()
        };
    
    public sealed class WebhookResponse
    {
        [JsonPropertyName("method")] public string Method = "sendMessage";

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
}