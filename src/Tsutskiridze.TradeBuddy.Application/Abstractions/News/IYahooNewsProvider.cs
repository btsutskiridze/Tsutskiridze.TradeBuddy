using Tsutskiridze.TradeBuddy.Application.Contracts.News;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News
{
    public interface IYahooNewsProvider
    {
        Task<List<YahooNewsItemDto>?> GetNewsAsync(string symbol, int? limit = null);
    }
}
