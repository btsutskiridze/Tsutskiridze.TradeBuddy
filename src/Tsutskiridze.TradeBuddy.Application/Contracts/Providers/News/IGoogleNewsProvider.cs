using Tsutskiridze.TradeBuddy.Application.Contracts.News;

namespace Tsutskiridze.TradeBuddy.Application.Contracts.Providers.News
{
    public interface IGoogleNewsProvider
    {
        Task<List<GoogleNewsItemDto>?> GetNewsAsync(string symbol, int? limit = null);
    }
}
