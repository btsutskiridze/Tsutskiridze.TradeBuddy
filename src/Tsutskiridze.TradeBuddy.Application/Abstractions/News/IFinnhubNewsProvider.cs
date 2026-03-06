using Tsutskiridze.TradeBuddy.Application.Dtos;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News
{
    public interface IFinnhubNewsProvider
    {
        Task<List<FinnhubNews>?> GetCompanyNewsAsync(string symbol, DateTime from, DateTime to, int? limit = null);
    }

}
