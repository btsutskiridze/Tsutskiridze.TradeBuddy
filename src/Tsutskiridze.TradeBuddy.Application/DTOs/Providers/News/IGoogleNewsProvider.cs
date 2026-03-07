using Tsutskiridze.TradeBuddy.Application.DTOs.News;

namespace Tsutskiridze.TradeBuddy.Application.DTOs.Providers.News
{
    public interface IGoogleNewsProvider
    {
        Task<List<GoogleNewsItemDto>?> GetNewsAsync(string symbol, int? limit = null);
    }
}
