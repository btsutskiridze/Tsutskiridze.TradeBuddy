using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Events;

public class PriceAlertCreatedEvent : DomainEvent
{
    public PriceAlertCreatedEvent(PriceAlert priceAlert)
    {
        PriceAlert = priceAlert;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public PriceAlert PriceAlert { get; private set; }
}