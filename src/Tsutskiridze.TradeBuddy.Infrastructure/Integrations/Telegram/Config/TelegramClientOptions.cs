namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram.Config
{
    public class TelegramClientOptions
    {
        public const string SectionName = "Telegram";
        public string BotToken { get; set; } = string.Empty;
    }
}

