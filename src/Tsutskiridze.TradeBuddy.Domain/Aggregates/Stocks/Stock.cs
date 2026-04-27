using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks.Events;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks;

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

        Symbol = NormalizeSymbol(symbol);
        Currency = currency;
        Name = string.IsNullOrEmpty(name) ? symbol : name;
    }

    private Stock()
    {
    }

    public void Watch()
    {
        if (IsWatched) return;
        
        IsWatched = true;
        RaiseDomainEvent(
            new StockWatchedDomainEvent(Symbol)
        );
    }
    
    public void UnWatch()
    {
        if (!IsWatched) return;
        
        IsWatched = false;
        RaiseDomainEvent(
            new StockUnwatchedDomainEvent(Symbol)
        );
    }
    
    private static string NormalizeSymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new DomainException("Stock symbol is required.");
        }

        return symbol.Trim().ToUpperInvariant();
    }
}
