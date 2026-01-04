using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram
{
    [Table("stocks")]
    public class Stock
    {
        [Key]
        public Guid ID { get; set; }

        [Required, MaxLength(20)]
        public string Symbol { get; set; } = "";

        [Required, MaxLength(10)]
        public string Currency { get; set; } = "";

        [Required, MaxLength(200)]
        public string Name { get; set; } = "";

        public bool IsWatched { get; set; }

        [InverseProperty(nameof(PriceAlert.Stock))]
        public List<PriceAlert>? PriceAlerts { get; set; }
    }
}
