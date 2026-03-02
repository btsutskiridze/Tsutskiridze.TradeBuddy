using Tsutskiridze.TradeBuddy.Application.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News
{
    public interface INewsProvider
    {
        // Method returns the standardized NewsArticle list
        Task<IEnumerable<object>> GetNewsAsync(string symbol, int? limit = null);

        // Property to identify the provider implementation
        NewsSourceType SourceType { get; }
    }
}
