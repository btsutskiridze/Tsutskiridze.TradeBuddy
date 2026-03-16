using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Events;

public sealed record PriceAlertActivatedDomainEvent(PriceAlert PriceAlert) : DomainEvent;