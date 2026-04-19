namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram
{
    public class TelegramBotOptions
    {
        public const string SectionName = "Telegram";
        public long GroupChatID { get; set; }
        public long PersonalChatID { get; set; }
    }
}
