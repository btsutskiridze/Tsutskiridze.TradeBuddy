using Mediator;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Event;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Streaming
{
    public interface ISubscriptionManager : INotificationHandler<StockWatchStatusChangedDomainEvent>
    {
        /// <summary>Load initial watched symbols and push to the transport.</summary>
        Task InitializeAsync(CancellationToken ct);
    }
}
