using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.Stocks.Events;

public sealed record StockWatchedDomainEvent(string Symbol) : DomainEvent
{
    public override string EventType => "stock.watched.v1";
}