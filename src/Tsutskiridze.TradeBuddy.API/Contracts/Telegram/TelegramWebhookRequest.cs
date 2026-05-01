namespace Tsutskiridze.TradeBuddy.API.Contracts.Telegram;
using System.Text.Json.Serialization;

public sealed class TelegramWebhookRequest
{
    [JsonPropertyName("update_id")]
    public long UpdateId { get; init; }
    public TelegramMessageRequest? Message { get; init; }
    
    public sealed class TelegramMessageRequest
    {
        public string? Text { get; init; }
        public TelegramChatRequest Chat { get; init; }
        public sealed class TelegramChatRequest
        {
            public long Id { get; init; }
        }
    }
}
