using Tsutskiridze.TradeBuddy.Application.Dtos.Reddit;
using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Interfaces.News
{
    public interface IRedditNewsProvider
    {
        Task<List<RedditPost>?> GetRedditPosts(string symbol, RedditSortType sort, int? limit = null);
    }
}
