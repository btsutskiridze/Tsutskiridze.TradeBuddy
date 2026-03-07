using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Specifications;

public sealed class ActiveAlertsByStockIdSpec : Specification<PriceAlert>
{
    public ActiveAlertsByStockIdSpec(Guid stockId)
    {
        Query.Where(x => x.StockId == stockId && x.IsActive);
    }
}