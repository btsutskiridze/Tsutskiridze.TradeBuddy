using SharedKernel;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;

namespace Tsutskiridze.TradeBuddy.Core.Events;

public class PriceAlertCreatedEvent : DomainEvent
{
    public PriceAlertCreatedEvent(PriceAlert priceAlert)
    {
        PriceAlert = priceAlert;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public PriceAlert PriceAlert { get; private set; }
}