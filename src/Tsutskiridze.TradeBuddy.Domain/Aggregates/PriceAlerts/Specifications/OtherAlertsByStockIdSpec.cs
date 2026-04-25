using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.Specifications;

public sealed class OtherAlertsByStockIdSpec : Specification<PriceAlert>
{
    public OtherAlertsByStockIdSpec(Guid stockId, Guid currentAlertId)
    {
        Query.Where(x => x.StockId == stockId && x.Id != currentAlertId);
    }
}