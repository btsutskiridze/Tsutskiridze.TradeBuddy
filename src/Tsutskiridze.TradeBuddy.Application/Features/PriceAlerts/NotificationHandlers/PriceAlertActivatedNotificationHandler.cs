using SharedKernel;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Utilities;
using Tsutskiridze.TradeBuddy.Application.Notifications.PriceAlerts;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.NotificationHandlers;

public class PriceAlertActivatedNotificationHandler : IApplicationNotificationHandler<PriceAlertActivatedNotification>
{
    private readonly ITelegramSender _sender;
    private readonly ICurrencySymbolProvider _symbolProvider;

    public PriceAlertActivatedNotificationHandler(ITelegramSender sender, ICurrencySymbolProvider symbolProvider)
    {
        _sender = sender;
        _symbolProvider = symbolProvider;
    }

    public async ValueTask Handle(PriceAlertActivatedNotification notification, CancellationToken cancellationToken)
    {
        var currencySymbol = _symbolProvider.GetSymbol(notification.Currency) ?? notification.Currency;

        await _sender.SendMessage(
            notification.ChatId,
            $"✅ Price alert set for {notification.Symbol} {notification.Direction} {currencySymbol}{notification.Price}"
            , cancellationToken);
    }
}