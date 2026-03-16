using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Events;

public sealed record PriceAlertDeactivatedDomainEvent(PriceAlert PriceAlert) : DomainEvent;