using System.Globalization;
using System.Text;
using SharedKernel;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring;
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
        var message = CreateMessage(notification.Summary);

        await _sender.Send(new TelegramOutgoingMessage(notification.Summary.ChatId, message, ParseMode.Markdown), ct);
    }

    private static string CreateMessage(StrategyMonitorEvaluationSummary result)
    {
        var evaluation = result.Evaluation;
        var sb = new StringBuilder();

        sb.AppendLine($"📊 *Strategy Monitor Alert*");
        sb.AppendLine($"Symbol: *{result.Symbol}* | Strategy: *{result.StrategyCode}*");
        sb.AppendLine();

        sb.AppendLine($"🕯 *Latest Candle*");
        sb.AppendLine($"Date: *{evaluation.CandleDate:yyyy-MM-dd}* | Close: *{FormatDecimal(evaluation.ClosePrice)}*");
        sb.AppendLine();

        sb.AppendLine($"⚡ *Signal*");
        sb.AppendLine($"Action: *{evaluation.Action}* | Position: *{evaluation.PositionSideAfter}*");
        sb.AppendLine();

        sb.AppendLine($"💰 *Execution Details*");
        sb.AppendLine($"Entry: *{FormatNullableDecimal(evaluation.ExecutionPrice)}* | Stop: *{FormatNullableDecimal(evaluation.ActiveStop)}*");

        if (result.LongProfitPercent is not null)
        {
            var profitEmoji = result.LongProfitPercent.Value >= 0 ? "🟢" : "🔴";
            sb.AppendLine($"{profitEmoji} Profit: *{result.LongProfitPercent.Value:N2}%*");
        }

        sb.AppendLine();
        sb.AppendLine($"📝 *Reason*");
        sb.AppendLine(evaluation.Reason);

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