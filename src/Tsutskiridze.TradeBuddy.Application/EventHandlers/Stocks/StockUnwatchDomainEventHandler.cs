using SharedKernel.Events;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks.Events;

namespace Tsutskiridze.TradeBuddy.Application.EventHandlers.Stocks;

public sealed class StockUnwatchDomainEventHandler : IDomainEventHandler<StockUnwatchedDomainEvent>
{
    private readonly IMarketDataListener _listener;

    public StockUnwatchDomainEventHandler(IMarketDataListener listener)
    {
        _listener = listener;
    }

    public async ValueTask Handle(StockUnwatchedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _listener.UnsubscribeStockPriceAsync(notification.Symbol, cancellationToken);
    }
}