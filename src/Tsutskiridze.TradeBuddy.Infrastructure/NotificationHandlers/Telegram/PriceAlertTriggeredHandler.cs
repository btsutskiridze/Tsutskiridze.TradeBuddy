using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.Localization;
using Tsutskiridze.TradeBuddy.Application.Notifications;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts.Enums;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram;

namespace Tsutskiridze.TradeBuddy.Infrastructure.NotificationHandlers.Telegram;

public sealed class PriceAlertTriggeredHandler : IBaseNotificationHandler<PriceAlertTriggeredNotification>
{
    private readonly ITelegramSender _sender;
    private readonly ILogger<PriceAlertTriggeredHandler> _logger;

    public PriceAlertTriggeredHandler(ITelegramSender sender,
        ILogger<PriceAlertTriggeredHandler> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async ValueTask Handle(PriceAlertTriggeredNotification notification, CancellationToken ct)
    {
        var currencySymbol = CurrencySymbolLookup.GetSymbol(notification.CurrencyCode) ?? notification.CurrencyCode;
        var directionEmoji = notification.Direction == PriceDirection.Above ? "🚀" : "📉";
        var directionText = notification.Direction == PriceDirection.Above ? "Above" : "Below";

        var triggerMessage =
            $"🔔 *{notification.Symbol}*: {currencySymbol}{notification.CurrentPrice:N2} 🔔\n" +
            $"{directionEmoji} {directionText} {currencySymbol}{notification.AlertPrice:N2}";
        await _sender.Send(new TelegramOutgoingMessage(notification.ChatId, triggerMessage), ct);
        
        if (notification.WasDeactivated)
        {
            var removedMessage =
                $"🚫 *{notification.Symbol}* alert removed after {notification.MaxNotifications} notifications.";
            await _sender.Send(new TelegramOutgoingMessage(notification.ChatId, removedMessage), ct);
        }

        _logger.LogInformation("Price alert notification delivery completed");
    }
}