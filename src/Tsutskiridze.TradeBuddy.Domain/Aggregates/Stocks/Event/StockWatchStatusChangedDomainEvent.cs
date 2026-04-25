using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks.Event;

public sealed record StockWatchStatusChangedDomainEvent(string Symbol, bool IsWatched) : DomainEvent;