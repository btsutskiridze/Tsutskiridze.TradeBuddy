using Mediator;
using SharedKernel;
using Tsutskiridze.TradeBuddy.Core.Enums;

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