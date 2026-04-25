using SharedKernel;
using SharedKernel.Events;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.Events;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks.Specifications;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.EventHandlers;

public sealed class PriceAlertDeactivatedDomainEventHandler : IDomainEventHandler<PriceAlertDeactivatedDomainEvent>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<Stock> _stocks;
    private readonly IReadRepository<PriceAlert> _alerts;

    public PriceAlertDeactivatedDomainEventHandler(
        IUnitOfWork uow,
        IRepository<Stock> stocks,
        IReadRepository<PriceAlert> alerts
    )
    {
        _uow = uow;
        _stocks = stocks;
        _alerts = alerts;
    }

    public async ValueTask Handle(PriceAlertDeactivatedDomainEvent notification, CancellationToken ct)
    {
        var stock = await _stocks.FirstOrDefaultAsync(new StocksByIdsSpec([notification.StockId]), ct);
        if (stock is null)
            return;

        var anyActiveAlerts = await _alerts.AnyAsync(new ActiveAlertsByStockIdSpec(notification.StockId), ct);
        if (anyActiveAlerts)
            return;

        stock.UnWatch();
        await _uow.SaveChangesAsync(ct);
    }
}