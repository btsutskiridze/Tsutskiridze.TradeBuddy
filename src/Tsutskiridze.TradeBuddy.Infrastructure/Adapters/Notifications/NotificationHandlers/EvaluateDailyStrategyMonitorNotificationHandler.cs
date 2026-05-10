using SharedKernel;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Notifications;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram.Formatting;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Adapters.Notifications.NotificationHandlers;

public class EvaluateDailyStrategyMonitorNotificationHandler : IBaseNotificationHandler<EvaluateDailyStrategyMonitorNotification>
{
    private readonly ITelegramSender _sender;

    public EvaluateDailyStrategyMonitorNotificationHandler(ITelegramSender sender)
    {
        _sender = sender;
    }

    public async ValueTask Handle(EvaluateDailyStrategyMonitorNotification notification,
        CancellationToken ct)
    {
        var message = StrategyMonitorTelegramMessageFormatter.CreateAlertMessage(notification.Summary);

        await _sender.Send(new TelegramOutgoingMessage(notification.Summary.ChatId, message, ParseMode.Markdown), ct);
    }
}