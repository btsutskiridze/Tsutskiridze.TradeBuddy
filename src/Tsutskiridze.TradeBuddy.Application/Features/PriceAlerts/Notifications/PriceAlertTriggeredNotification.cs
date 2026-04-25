using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Notifications;

public sealed record PriceAlertTriggeredNotification(
    long ChatId,
    string Symbol,
    string CurrencyCode,
    decimal CurrentPrice,
    decimal AlertPrice,
    PriceDirection Direction,
    bool WasDeactivated,
    int MaxNotifications):IBaseNotification;