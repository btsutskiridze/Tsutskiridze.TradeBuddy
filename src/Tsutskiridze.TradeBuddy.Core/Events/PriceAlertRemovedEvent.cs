using SharedKernel;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;

namespace Tsutskiridze.TradeBuddy.Core.Events;

public class PriceAlertRemovedEvent : DomainEvent
{
    public PriceAlertRemovedEvent(PriceAlert priceAlert)
    {
        PriceAlert = priceAlert;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public PriceAlert PriceAlert { get; private set; }
}