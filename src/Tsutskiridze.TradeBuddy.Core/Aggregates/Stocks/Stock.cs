using SharedKernel;
using Tsutskiridze.TradeBuddy.Core.Events;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;

public class Stock : Entity<Guid>, IAggregateRoot
{
    public string Symbol { get; private init; }
    public string Currency { get; private init; }
    public string Name { get; private init; }
    public bool IsWatched { get; private set; }

    public Stock(string symbol, string currency, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        ArgumentException.ThrowIfNullOrEmpty(currency);
        ArgumentException.ThrowIfNullOrEmpty(name);

        Symbol = symbol;
        Currency = currency;
        Name = name;
    }

    private Stock()
    {
    }

    public void Watch()
    {
        if (IsWatched) return;
        
        IsWatched = true;
        RaiseDomainEvent(
            new StockWatchStatusChangedEvent(Symbol, IsWatched)
        );
    }
    
    public void UnWatch()
    {
        if (!IsWatched) return;
        
        IsWatched = false;
        RaiseDomainEvent(
            new StockWatchStatusChangedEvent(Symbol, IsWatched)
        );
    }
}
