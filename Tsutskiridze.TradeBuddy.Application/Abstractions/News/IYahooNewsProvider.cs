using Tsutskiridze.TradeBuddy.Application.Dtos.Yahoo;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News
{
    public interface IYahooNewsProvider
    {
        Task<List<YahooNews>?> GetNewsAsync(string symbol, int? limit = null);
    }
}
