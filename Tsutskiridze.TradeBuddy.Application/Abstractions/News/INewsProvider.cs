using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Interfaces.News
{
    public interface INewsProvider
    {
        // Method returns the standardized NewsArticle list
        Task<IEnumerable<object>> GetNewsAsync(string symbol, int? limit = null);

        // Property to identify the provider implementation
        NewsSourceType SourceType { get; }
    }
}
