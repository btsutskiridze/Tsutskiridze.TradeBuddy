using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Events;

public sealed record PriceAlertCreatedEvent(PriceAlert PriceAlert) : DomainEvent;