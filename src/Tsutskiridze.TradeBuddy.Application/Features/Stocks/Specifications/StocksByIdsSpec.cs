using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks;

namespace Tsutskiridze.TradeBuddy.Application.Features.Stocks.Specifications;

public sealed class StocksByIdsSpec : Specification<Stock>
{
    public StocksByIdsSpec(IReadOnlyCollection<Guid> ids)
    {
        Query.Where(x => ids.Contains(x.Id));
    }
}