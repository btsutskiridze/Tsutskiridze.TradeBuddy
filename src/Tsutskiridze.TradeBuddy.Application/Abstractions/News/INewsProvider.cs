using Tsutskiridze.TradeBuddy.Application.Common.Enums;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News;

public interface INewsProvider
{
    Task<StockNewsDto> GetAllNews(string symbol, int? limit = null);
    Task<List<GoogleNewsItemDto>?> GetGoogleNews(string symbol, int? limit = null);
    Task<List<RedditPostDto>?> GetRedditNews(string symbol, RedditSortType sort, int? limit = null);
    Task<List<YahooNewsItemDto>?> GetYahooNews(string symbol, int? limit = null);
    Task<List<FinnhubNewsItemDto>?> GetFinnhubNews(string symbol, DateTime from, DateTime to, int? limit = null);
}