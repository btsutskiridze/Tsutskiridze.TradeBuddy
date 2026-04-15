using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Events;

public sealed record PriceAlertDeactivatedDomainEvent(Guid StockId) : DomainEvent;