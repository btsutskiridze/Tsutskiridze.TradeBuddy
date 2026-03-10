using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Events;

public sealed record PriceAlertRemovedEvent(PriceAlert PriceAlert) : DomainEvent;