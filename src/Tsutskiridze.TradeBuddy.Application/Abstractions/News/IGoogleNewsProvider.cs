using Tsutskiridze.TradeBuddy.Application.Dtos.Google;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News
{
    public interface IGoogleNewsProvider
    {
        Task<List<GoogleNews>?> GetNewsAsync(string symbol, int? limit = null);
    }
}
