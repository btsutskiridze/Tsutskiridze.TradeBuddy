using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts.Enums;

namespace Tsutskiridze.TradeBuddy.Domain.PriceAlerts.Events;

public sealed record PriceAlertTriggeredDomainEvent(
    Guid PriceAlertId,
    Guid ChatId,
    Guid StockId,
    decimal AlertPrice,
    decimal CurrentPrice,
    PriceDirection Direction,
    bool WasDeactivated,
    int MaxNotifications) : DomainEvent
{
    public override string EventType => "price_alert.triggered.v1";
}