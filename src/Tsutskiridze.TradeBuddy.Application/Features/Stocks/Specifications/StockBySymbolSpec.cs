using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks;

namespace Tsutskiridze.TradeBuddy.Application.Features.Stocks.Specifications;

public sealed class StockBySymbolSpec : Specification<Stock>
{
    public StockBySymbolSpec(string symbol)
    {
        Query.Where(x => x.Symbol == symbol);
    }
}