using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.Specifications;

public sealed class ActiveAlertsByStockIdSpec : Specification<PriceAlert>
{
    public ActiveAlertsByStockIdSpec(Guid stockId)
    {
        Query.Where(x => x.StockId == stockId && x.IsActive);
    }
}