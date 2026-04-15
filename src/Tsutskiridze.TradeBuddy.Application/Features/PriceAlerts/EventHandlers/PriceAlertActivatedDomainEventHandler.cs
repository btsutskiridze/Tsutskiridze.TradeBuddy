using SharedKernel;
using SharedKernel.Events;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Events;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Specifications;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.EventHandlers;

public sealed class PriceAlertActivatedDomainEventHandler : IDomainEventHandler<PriceAlertActivatedDomainEvent>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<Stock> _stocks;

    public PriceAlertActivatedDomainEventHandler(IUnitOfWork uow, IRepository<Stock> stocks)
    {
        _uow = uow;
        _stocks = stocks;
    }

    public async ValueTask Handle(PriceAlertActivatedDomainEvent notification, CancellationToken ct)
    {
        var stock = await _stocks.FirstOrDefaultAsync(new StocksByIdsSpec([notification.StockId]), ct);

        if (stock is null)
            return;

        stock.Watch();
        await _uow.SaveChangesAsync(ct);
    }
}