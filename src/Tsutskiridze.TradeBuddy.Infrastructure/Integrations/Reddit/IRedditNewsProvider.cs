using Tsutskiridze.TradeBuddy.Application.Common.Enums;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit;

public interface IRedditNewsProvider
{
    Task<List<RedditPost>?> GetRedditPosts(string symbol, SortType sortType, int? limit = null);
}
