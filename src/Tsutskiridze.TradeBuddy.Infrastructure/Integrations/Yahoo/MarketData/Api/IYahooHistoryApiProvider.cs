using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Api;

public interface IYahooHistoryApiProvider
{
    Task<IReadOnlyList<MarketCandle>> GetDailyCandles(string symbol, DateOnly from, DateOnly to, CancellationToken ct);
    
    Task<MarketHistoryDateRange> GetClosedDailyDateRange(
        string symbol,
        DateTimeOffset nowUtc,
        CancellationToken ct);
}