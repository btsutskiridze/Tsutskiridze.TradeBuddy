namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram
{
    public class TelegramClientOptions
    {
        public const string SectionName = "Telegram";
        public string BotToken { get; set; } = string.Empty;
    }
}

