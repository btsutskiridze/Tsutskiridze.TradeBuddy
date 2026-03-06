using Tsutskiridze.TradeBuddy.Application.Dtos;
using Tsutskiridze.TradeBuddy.Application.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News
{
    public interface IRedditNewsProvider
    {
        Task<List<RedditPost>?> GetRedditPosts(string symbol, RedditSortType sort, int? limit = null);
    }
}
