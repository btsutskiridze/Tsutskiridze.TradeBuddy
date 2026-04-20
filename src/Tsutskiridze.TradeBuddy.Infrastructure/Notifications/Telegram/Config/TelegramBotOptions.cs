namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Config
{
    public class TelegramBotOptions
    {
        public const string SectionName = "Telegram";
        public long GroupChatID { get; set; }
        public long PersonalChatID { get; set; }
    }
}
