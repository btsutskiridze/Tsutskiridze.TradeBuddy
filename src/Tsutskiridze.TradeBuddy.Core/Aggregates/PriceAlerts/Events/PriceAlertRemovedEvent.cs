using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Events;

public class PriceAlertRemovedEvent : DomainEvent
{
    public PriceAlertRemovedEvent(PriceAlert priceAlert)
    {
        PriceAlert = priceAlert;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public PriceAlert PriceAlert { get; private set; }
}