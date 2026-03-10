using Tsutskiridze.TradeBuddy.Application.DTOs.News;
using Tsutskiridze.TradeBuddy.Application.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News;

public interface INewsAggregator
{
    Task<StockNewsDto> GetAllNews(string symbol, int? limit = null);
    Task<List<GoogleNewsItemDto>?> GetGoogleNews(string symbol, int? limit = null);
    Task<List<RedditPostDto>?> GetRedditNews(string symbol, RedditSortType sort, int? limit = null);
    Task<List<YahooNewsItemDto>?> GetYahooNews(string symbol, int? limit = null);
    Task<List<FinnhubNewsItemDto>?> GetFinnhubNews(string symbol, DateTime from, DateTime to, int? limit = null);
}