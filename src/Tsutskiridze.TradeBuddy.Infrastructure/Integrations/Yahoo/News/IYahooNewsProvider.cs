using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.News.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.News;

public interface IYahooNewsProvider
{
    Task<List<YahooNewsItem>?> GetNewsAsync(string symbol, int? limit = null);
}
