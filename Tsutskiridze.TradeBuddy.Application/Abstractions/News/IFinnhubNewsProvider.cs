using Tsutskiridze.TradeBuddy.Application.Dtos.Finnhub;

namespace Tsutskiridze.TradeBuddy.Application.Interfaces.News
{
    public interface IFinnhubNewsProvider
    {
        Task<List<FinnhubNews>?> GetCompanyNewsAsync(string symbol, DateTime from, DateTime to, int? limit = null);
    }

}
