using Mediator;
using Tsutskiridze.TradeBuddy.Core.Events;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.StockMarket
{
    public interface ISubscriptionManager : INotificationHandler<StockWatchStatusChangedEvent>
    {
        /// <summary>Load initial watched symbols and push to the transport.</summary>
        Task InitializeAsync(CancellationToken ct);
    }
}
