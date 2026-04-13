using Tsutskiridze.TradeBuddy.Application.DTOs.News;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News
{
    // todo: make them abstract and not direct providers in application layer
    public interface IFinnhubNewsProvider
    {
        Task<List<FinnhubNewsItemDto>?> GetCompanyNewsAsync(string symbol, DateTime from, DateTime to, int? limit = null);
    }
}
