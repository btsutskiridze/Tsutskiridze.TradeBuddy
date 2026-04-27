using SharedKernel.Events;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks.Events;

namespace Tsutskiridze.TradeBuddy.Application.EventHandlers.Stocks;

public sealed class StockWatchDomainEventHandler : IDomainEventHandler<StockWatchedDomainEvent>
{
    private readonly IMarketDataListener _listener;

    public StockWatchDomainEventHandler(IMarketDataListener listener)
    {
        _listener = listener;
    }

    public async ValueTask Handle(StockWatchedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _listener.SubscribeStockPriceAsync(notification.Symbol, cancellationToken);
    }
}