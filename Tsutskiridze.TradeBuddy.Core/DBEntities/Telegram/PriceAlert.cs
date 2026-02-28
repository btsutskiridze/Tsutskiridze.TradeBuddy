using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram
{
    public class PriceAlert
    {
        public Guid ID { get; set; }

        public Guid ChatID { get; set; }

        public Guid StockID { get; set; }

        public decimal Price { get; set; }

        public PriceAlertDirection Direction { get; set; }

        public int AlertCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Chat Chat { get; set; } = null!;
        public Stock Stock { get; set; } = null!;
    }
}
