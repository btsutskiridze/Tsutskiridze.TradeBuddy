namespace Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram
{
    public class Chat
    {
        public Guid ID { get; set; }

        public long? TelegramChatID { get; set; }

        public string? PrivateName { get; set; }

        public string? ActivationToken { get; set; }

        public List<PriceAlert> PriceAlerts { get; } = new();
    }
}
