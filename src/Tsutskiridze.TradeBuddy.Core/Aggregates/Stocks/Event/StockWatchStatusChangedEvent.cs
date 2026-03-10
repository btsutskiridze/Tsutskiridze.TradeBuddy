using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Event;

public sealed record StockWatchStatusChangedEvent(string Symbol, bool IsWatched) : DomainEvent;