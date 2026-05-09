using System.Globalization;
using System.Text;
using SharedKernel;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Notifications;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram;

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
        var message = CreateMessage(notification);

        await _sender.Send(new TelegramOutgoingMessage(notification.ChatId, message, ParseMode.Markdown), ct);
    }

    private static string CreateMessage(EvaluateDailyStrategyMonitorNotification result)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"📊 *Strategy Monitor Alert*");
        sb.AppendLine($"Symbol: *{result.Symbol}* | Strategy: *{result.StrategyCode}*");
        sb.AppendLine();

        sb.AppendLine($"🕯 *Latest Candle*");
        sb.AppendLine($"Date: *{result.CandleDate:yyyy-MM-dd}* | Close: *{FormatDecimal(result.ClosePrice)}*");
        sb.AppendLine();

        sb.AppendLine($"⚡ *Signal*");
        sb.AppendLine($"Action: *{result.Action}* | Position: *{result.PositionSideAfter}*");
        sb.AppendLine();

        sb.AppendLine($"💰 *Execution Details*");
        sb.AppendLine($"Entry: *{FormatNullableDecimal(result.ExecutionPrice)}* | Stop: *{FormatNullableDecimal(result.ActiveStop)}*");

        if (result.LongProfitPercent is not null)
        {
            var profitEmoji = result.LongProfitPercent.Value >= 0 ? "🟢" : "🔴";
            sb.AppendLine($"{profitEmoji} Profit: *{result.LongProfitPercent.Value:N2}%*");
        }

        sb.AppendLine();
        sb.AppendLine($"📝 *Reason*");
        sb.AppendLine(result.Reason);

        return sb.ToString();
    }

    //todo: refactor those methods are duplicated at couple of places
    private static string FormatNullableDecimal(decimal? value)
    {
        return value.HasValue
            ? FormatDecimal(value.Value)
            : "n/a";
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.####", CultureInfo.InvariantCulture);
    }
}