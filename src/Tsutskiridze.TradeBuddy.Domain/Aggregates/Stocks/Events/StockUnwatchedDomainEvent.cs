using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks.Events;

public sealed record StockUnwatchedDomainEvent(string Symbol) : DomainEvent;