using SharedKernel;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Notifications;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram;
using Tsutskiridze.TradeBuddy.Infrastructure.TextFormatting.Telegram;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Adapters.Notifications.NotificationHandlers;

public class StrategyMonitorAlertNotificationHandler : IBaseNotificationHandler<StrategyMonitorAlertNotification>
{
    private readonly ITelegramSender _sender;

    public StrategyMonitorAlertNotificationHandler(ITelegramSender sender)
    {
        _sender = sender;
    }

    public async ValueTask Handle(StrategyMonitorAlertNotification alertNotification,
        CancellationToken ct)
    {
        var message = StrategyMonitorMessageFormatter.CreateAlertMessage(alertNotification.Summary);

        await _sender.Send(new TelegramOutgoingMessage(alertNotification.Summary.ChatId, message, ParseMode.Markdown), ct);
    }
}