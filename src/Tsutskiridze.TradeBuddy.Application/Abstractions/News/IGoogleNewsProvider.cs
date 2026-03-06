using Tsutskiridze.TradeBuddy.Application.Contracts.News;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News
{
    public interface IGoogleNewsProvider
    {
        Task<List<GoogleNewsItemDto>?> GetNewsAsync(string symbol, int? limit = null);
    }
}
