using Tsutskiridze.TradeBuddy.Application.Contracts.News;
using Tsutskiridze.TradeBuddy.Application.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Contracts.Providers.News
{
    public interface IRedditNewsProvider
    {
        Task<List<RedditPostDto>?> GetRedditPosts(string symbol, RedditSortType sort, int? limit = null);
    }
}
