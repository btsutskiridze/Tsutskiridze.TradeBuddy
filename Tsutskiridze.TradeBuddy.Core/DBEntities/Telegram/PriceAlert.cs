using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram
{
    [Table("price_alerts")]
    public class PriceAlert
    {
        [Key]
        public Guid ID { get; set; }

        [Required, ForeignKey(nameof(Chat))]
        public Guid ChatID { get; set; }

        [Required, ForeignKey(nameof(Stock))]
        public Guid StockID { get; set; }

        [Required]
        [Column(TypeName = "numeric")]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "numeric")]
        public PriceAlertDirection Direction { get; set; }

        [Required, Column(TypeName = "int")]
        public int AlertCount { get; set; } = 0;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Chat Chat { get; set; } = null!;
        public Stock Stock { get; set; } = null!;
    }
}
