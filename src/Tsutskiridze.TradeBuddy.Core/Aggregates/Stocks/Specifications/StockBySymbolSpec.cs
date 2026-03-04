using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Specifications;

public sealed class StockBySymbolSpec : Specification<Stock>
{
    public StockBySymbolSpec(string symbol)
    {
        Query(query => { return query.Where(x => x.Symbol == symbol); });
    }
}