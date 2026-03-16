using SharedKernel;
using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Notifications.PriceAlerts;

public sealed record PriceAlertActivatedNotification(long ChatId, string Symbol, PriceDirection Direction, string Currency, decimal Price) : IApplicationNotification
{
}