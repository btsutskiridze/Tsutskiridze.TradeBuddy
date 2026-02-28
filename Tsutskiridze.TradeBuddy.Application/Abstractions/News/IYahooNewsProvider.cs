using Tsutskiridze.TradeBuddy.Application.Dtos.Yahoo;

namespace Tsutskiridze.TradeBuddy.Application.Interfaces.News
{
    public interface IYahooNewsProvider
    {
        Task<List<YahooNews>?> GetNewsAsync(string symbol, int? limit = null);
    }
}
