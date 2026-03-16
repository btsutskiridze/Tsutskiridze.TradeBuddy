using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Event;

public sealed record StockWatchStatusChangedDomainEvent(string Symbol, bool IsWatched) : DomainEvent;