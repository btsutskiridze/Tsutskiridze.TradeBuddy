using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Specifications;

public sealed class ActiveAlertsByStockIdExceptAlertSpec : Specification<PriceAlert>
{
    public ActiveAlertsByStockIdExceptAlertSpec(Guid stockId, Guid alertId)
    {
        Query.Where(x => x.StockId == stockId && x.IsActive && x.Id != alertId);
    }
}