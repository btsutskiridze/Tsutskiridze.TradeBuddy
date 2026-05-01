using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.GoogleNews.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.GoogleNews;

public interface IGoogleNewsProvider
{
    Task<List<GoogleNewsItem>?> GetNewsAsync(string symbol, int? limit = null);
}
