using SharedKernel;
using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Specifications;

public sealed class OtherAlertsByStockIdSpec : Specification<PriceAlert>
{
    public OtherAlertsByStockIdSpec(Guid stockId, Guid currentAlertId)
    {
        Query.Where(x => x.StockId == stockId && x.Id != currentAlertId);
    }
}