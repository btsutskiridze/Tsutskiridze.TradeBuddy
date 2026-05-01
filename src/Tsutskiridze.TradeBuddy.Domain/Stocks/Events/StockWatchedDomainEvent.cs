using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.Stocks.Events;

public sealed record StockWatchedDomainEvent(string Symbol) : DomainEvent;