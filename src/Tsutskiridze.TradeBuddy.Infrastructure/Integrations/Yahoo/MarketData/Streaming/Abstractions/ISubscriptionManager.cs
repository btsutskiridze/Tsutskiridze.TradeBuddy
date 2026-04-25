using Mediator;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks.Event;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Streaming.Abstractions
{
    public interface ISubscriptionManager : INotificationHandler<StockWatchStatusChangedDomainEvent>
    {
        /// <summary>Load initial watched symbols and push to the transport.</summary>
        Task InitializeAsync(CancellationToken ct);
    }
}
