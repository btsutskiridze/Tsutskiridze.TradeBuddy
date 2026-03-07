using Tsutskiridze.TradeBuddy.Application.DTOs.News;
using Tsutskiridze.TradeBuddy.Application.Enums;

namespace Tsutskiridze.TradeBuddy.Application.DTOs.Providers.News
{
    public interface IRedditNewsProvider
    {
        Task<List<RedditPostDto>?> GetRedditPosts(string symbol, RedditSortType sort, int? limit = null);
    }
}
