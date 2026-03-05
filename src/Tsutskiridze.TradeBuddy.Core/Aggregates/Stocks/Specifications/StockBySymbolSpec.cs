using SharedKernel;
using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Specifications;

public sealed class StockBySymbolSpec : Specification<Stock>
{
    public StockBySymbolSpec(string symbol)
    {
        Query.Where(x => x.Symbol == symbol);
    }
}