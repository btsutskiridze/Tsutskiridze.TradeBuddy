using Mediator;

namespace Tsutskiridze.TradeBuddy.Application.Events
{
    public sealed record StockWatchStatusChanged(string Symbol, bool IsWatched) : INotification;
}
