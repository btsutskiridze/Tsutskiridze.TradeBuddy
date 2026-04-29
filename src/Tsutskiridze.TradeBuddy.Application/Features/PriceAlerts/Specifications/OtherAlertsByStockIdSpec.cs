using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Specifications;

public sealed class OtherAlertsByStockIdSpec : Specification<PriceAlert>
{
    public OtherAlertsByStockIdSpec(Guid stockId, Guid currentAlertId)
    {
        Query.Where(x => x.StockId == stockId && x.Id != currentAlertId);
    }
}