using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Notifications;

public sealed record PriceAlertTriggeredNotification(
    long ChatId,
    string Symbol,
    string CurrencyCode,
    decimal CurrentPrice,
    decimal AlertPrice,
    PriceDirection Direction,
    bool WasDeactivated,
    int MaxNotifications):IBaseNotification;