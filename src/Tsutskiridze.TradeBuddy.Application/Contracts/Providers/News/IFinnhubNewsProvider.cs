using Tsutskiridze.TradeBuddy.Application.Contracts.News;

namespace Tsutskiridze.TradeBuddy.Application.Contracts.Providers.News
{
    public interface IFinnhubNewsProvider
    {
        Task<List<FinnhubNewsItemDto>?> GetCompanyNewsAsync(string symbol, DateTime from, DateTime to, int? limit = null);
    }
}
