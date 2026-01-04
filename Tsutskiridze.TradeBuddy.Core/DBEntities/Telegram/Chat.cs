using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram
{
    [Table("chats")]
    public class Chat
    {
        [Key]
        public Guid ID { get; set; }

        public long? TelegramChatID { get; set; }

        public string? PrivateName { get; set; }

        public string? ActivationToken { get; set; }

        [InverseProperty(nameof(PriceAlert.Chat))]
        public List<PriceAlert> PriceAlerts { get; } = new();
    }
}
