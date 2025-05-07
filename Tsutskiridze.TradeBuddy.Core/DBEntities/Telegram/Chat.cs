using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram
{
    [Table("chats")]
    public class Chat
    {
        [Key]
        public Guid ID { get; set; }

        [Required]
        public long TelegramChatID { get; set; }

        [InverseProperty(nameof(PriceAlert.Chat))]
        public List<PriceAlert> PriceAlerts { get; } = new();
    }
}
