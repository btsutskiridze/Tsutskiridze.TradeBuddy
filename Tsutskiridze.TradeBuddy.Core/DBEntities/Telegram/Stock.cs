namespace Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram
{
    public class Stock
    {
        public Guid ID { get; set; }

        public string Symbol { get; set; } = "";

        public string Currency { get; set; } = "";

        public string Name { get; set; } = "";

        public bool IsWatched { get; set; }

        public List<PriceAlert>? PriceAlerts { get; set; }
    }
}
