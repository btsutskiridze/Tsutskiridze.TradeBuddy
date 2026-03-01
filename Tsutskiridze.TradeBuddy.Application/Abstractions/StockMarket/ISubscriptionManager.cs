using Mediator;
using Tsutskiridze.TradeBuddy.Application.Events;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.StockMarket
{
    public interface ISubscriptionManager : INotificationHandler<StockWatchStatusChanged>
    {
        /// <summary>Load initial watched symbols and push to the transport.</summary>
        Task InitializeAsync(CancellationToken ct);
    }
}
