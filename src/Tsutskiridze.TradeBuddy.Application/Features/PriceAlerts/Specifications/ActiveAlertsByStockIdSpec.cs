using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Specifications;

public sealed class ActiveAlertsByStockIdSpec : Specification<PriceAlert>
{
    public ActiveAlertsByStockIdSpec(Guid stockId)
    {
        Query.Where(x => x.StockId == stockId && x.IsActive);
    }
}