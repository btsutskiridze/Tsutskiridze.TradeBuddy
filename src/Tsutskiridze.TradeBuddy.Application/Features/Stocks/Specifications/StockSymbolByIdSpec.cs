using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Stocks;

namespace Tsutskiridze.TradeBuddy.Application.Features.Stocks.Specifications;

public record StockSymbolResult(string Symbol, string Currency);

public sealed class StockSymbolByIdSpec : Specification<Stock, Guid, StockSymbolResult>
{
    public StockSymbolByIdSpec(Guid stockId)
    {
        Query.Select(x => new StockSymbolResult(x.Symbol, x.Currency))
            .Where(x => x.Id == stockId);
    }
}