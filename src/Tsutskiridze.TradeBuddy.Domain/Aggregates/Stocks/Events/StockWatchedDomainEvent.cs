using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks.Events;

public sealed record StockWatchedDomainEvent(string Symbol) : DomainEvent;