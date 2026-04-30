using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.Stocks.Events;

public sealed record StockUnwatchedDomainEvent(string Symbol) : DomainEvent;