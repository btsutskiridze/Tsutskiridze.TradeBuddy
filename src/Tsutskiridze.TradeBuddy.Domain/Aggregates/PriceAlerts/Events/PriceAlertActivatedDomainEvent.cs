using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.Events;

public sealed record PriceAlertActivatedDomainEvent(Guid StockId) : DomainEvent;