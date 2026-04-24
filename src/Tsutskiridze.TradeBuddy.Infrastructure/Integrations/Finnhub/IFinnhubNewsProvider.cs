using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Finnhub.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Finnhub;

public interface IFinnhubNewsProvider
{
    Task<List<FinnhubNewsItem>?> GetCompanyNewsAsync(
        string symbol,
        DateTime from,
        DateTime to,
        int? limit = null);
}
