using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Event;

public class StockWatchStatusChangedEvent : DomainEvent
{
    public StockWatchStatusChangedEvent(string symbol, bool isWatched)
    {
        Symbol = symbol;
        IsWatched = isWatched;
    }

    public Guid Id = Guid.NewGuid();
    public string Symbol { get; private init; }
    public bool IsWatched { get; private init; }
}