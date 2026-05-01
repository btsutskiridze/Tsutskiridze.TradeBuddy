using Tsutskiridze.TradeBuddy.Application.Common.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News
{
    public class StockNews
    {
        public IReadOnlyCollection<StockNewsItem> Items { get; set; }

        public class StockNewsItem
        {
            public string Title { get; set; }
            public string Url { get; set; }
            public string Summary { get; set; }
            public string PublishTime { get; set; }
            public NewsSource Source { get; set; }
            public IReadOnlyDictionary<string, string> Metadata { get; init; }

            public StockNewsItem()
            {
                Metadata = new Dictionary<string, string>();
            }
        }
    }
}