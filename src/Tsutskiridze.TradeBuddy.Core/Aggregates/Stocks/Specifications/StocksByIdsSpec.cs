using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Specifications;

public sealed class StocksByIdsSpec : Specification<Stock>
{
    public StocksByIdsSpec(IReadOnlyCollection<Guid> ids)
    {
        Query.Where(x => ids.Contains(x.Id));
    }
}