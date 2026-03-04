using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Specifications;

public class StocksByIdsSpec:Specification<Stock>
{
    public StocksByIdsSpec(IReadOnlyCollection<Guid> ids)
    {
        Query(query => query.Where(x => ids.Contains(x.Id)));
    }
}