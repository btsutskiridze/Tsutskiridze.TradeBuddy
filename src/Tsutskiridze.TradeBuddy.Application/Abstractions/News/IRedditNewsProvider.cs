using Tsutskiridze.TradeBuddy.Application.Common.Enums;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News
{
    public interface IRedditNewsProvider
    {
        Task<List<RedditPostDto>?> GetRedditPosts(string symbol, RedditSortType sort, int? limit = null);
    }
}
