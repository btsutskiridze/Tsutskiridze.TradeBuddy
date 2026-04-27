using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.Specifications;

public sealed class ActiveAlertsByStockIdExceptAlertSpec : Specification<PriceAlert>
{
    public ActiveAlertsByStockIdExceptAlertSpec(Guid stockId, Guid alertId)
    {
        Query.Where(x => x.StockId == stockId && x.IsActive && x.Id != alertId);
    }
}