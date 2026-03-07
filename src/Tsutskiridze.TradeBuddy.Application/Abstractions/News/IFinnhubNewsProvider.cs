using Tsutskiridze.TradeBuddy.Application.DTOs.News;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News
{
    public interface IFinnhubNewsProvider
    {
        Task<List<FinnhubNewsItemDto>?> GetCompanyNewsAsync(string symbol, DateTime from, DateTime to, int? limit = null);
    }
}
