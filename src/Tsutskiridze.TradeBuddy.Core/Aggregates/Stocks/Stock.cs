using SharedKernel;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Event;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;

public class Stock : Entity<Guid>, IAggregateRoot
{
    public string Symbol { get; private init; }
    public string Currency { get; private init; }
    public string Name { get; private init; }
    public bool IsWatched { get; private set; }
    
    public uint Version { get; private set; }

    public Stock(string symbol, string currency, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol);
        ArgumentException.ThrowIfNullOrEmpty(currency);

        Symbol = symbol;
        Currency = currency;

        if (string.IsNullOrEmpty(name))
            Name = symbol;
    }

    private Stock()
    {
    }

    public void Watch()
    {
        if (IsWatched) return;
        
        IsWatched = true;
        RaiseDomainEvent(
            new StockWatchStatusChangedDomainEvent(Symbol, IsWatched)
        );
    }
    
    public void UnWatch()
    {
        if (!IsWatched) return;
        
        IsWatched = false;
        RaiseDomainEvent(
            new StockWatchStatusChangedDomainEvent(Symbol, IsWatched)
        );
    }
}
