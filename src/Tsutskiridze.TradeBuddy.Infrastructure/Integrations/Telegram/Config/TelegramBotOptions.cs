namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram.Config
{
    public class TelegramBotOptions
    {
        public const string SectionName = "Telegram";
        public long GroupChatID { get; set; }
        public long PersonalChatID { get; set; }
    }
}
