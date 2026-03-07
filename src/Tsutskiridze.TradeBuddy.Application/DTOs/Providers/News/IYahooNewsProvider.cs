using Tsutskiridze.TradeBuddy.Application.DTOs.News;

namespace Tsutskiridze.TradeBuddy.Application.DTOs.Providers.News
{
    public interface IYahooNewsProvider
    {
        Task<List<YahooNewsItemDto>?> GetNewsAsync(string symbol, int? limit = null);
    }
}
