using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Specifications;

public class OtherAlertsByStockIdSpec : Specification<PriceAlert>
{
    public OtherAlertsByStockIdSpec(Guid stockId, Guid currentAlertId)
    {
        Query(query => query.Where(x => x.StockId == stockId && x.Id != stockId && x.IsActive));
    }
}