using Tsutskiridze.TradeBuddy.Domain.Stocks;

namespace Tsutskiridze.TradeBuddy.Application.Features.Stocks.Services;

public interface IStockService
{
    Task<Stock> GetOrCreateStock(string symbol, string name, string currency, CancellationToken ct = default);
}