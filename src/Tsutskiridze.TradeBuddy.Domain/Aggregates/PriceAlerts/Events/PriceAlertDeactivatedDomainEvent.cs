using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.Events;

public sealed record PriceAlertDeactivatedDomainEvent(Guid StockId) : DomainEvent;