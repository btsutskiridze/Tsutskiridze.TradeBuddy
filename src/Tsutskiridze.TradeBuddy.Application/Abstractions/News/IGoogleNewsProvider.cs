using Tsutskiridze.TradeBuddy.Application.DTOs.News;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News
{
    public interface IGoogleNewsProvider
    {
        Task<List<GoogleNewsItemDto>?> GetNewsAsync(string symbol, int? limit = null);
    }
}
