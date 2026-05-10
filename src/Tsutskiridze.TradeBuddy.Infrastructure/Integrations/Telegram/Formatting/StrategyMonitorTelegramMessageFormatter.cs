using System.Text;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram.Formatting;

public static class StrategyMonitorTelegramMessageFormatter
{
    public static string CreateActivationMessage(StrategyMonitorEvaluationSummary summary)
    {
        var evaluation = summary.Evaluation;
        var sb = new StringBuilder();

        sb.AppendLine($"Strategy monitor activated for `{summary.Symbol}` with `{summary.StrategyCode}`.");
        sb.AppendLine(
            $"Latest candle: `{evaluation.CandleDate:yyyy-MM-dd}` close `{TelegramValueFormatter.Decimal(evaluation.ClosePrice)}`.");

        if (summary.LongProfitPercent is not null)
            sb.AppendLine($"Profit: `{TelegramValueFormatter.Percent(summary.LongProfitPercent.Value)}`.");

        sb.AppendLine($"Action: `{evaluation.Action}`. Position: `{evaluation.PositionSideAfter}`.");
        sb.AppendLine(
            $"Execution: `{TelegramValueFormatter.NullableDecimal(evaluation.ExecutionPrice)}`. Stop: `{TelegramValueFormatter.NullableDecimal(evaluation.ActiveStop)}`.");
        sb.AppendLine($"Reason: {evaluation.Reason}");

        return sb.ToString();
    }

    public static string CreateAlertMessage(StrategyMonitorEvaluationSummary summary)
    {
        var evaluation = summary.Evaluation;
        var sb = new StringBuilder();

        sb.AppendLine($"📊 *Strategy Monitor Alert*");
        sb.AppendLine($"Symbol: *{summary.Symbol}* | Strategy: *{summary.StrategyCode}*");
        sb.AppendLine();

        sb.AppendLine($"🕯 *Latest Candle*");
        sb.AppendLine(
            $"Date: *{evaluation.CandleDate:yyyy-MM-dd}* | Close: *{TelegramValueFormatter.Decimal(evaluation.ClosePrice)}*");
        sb.AppendLine();

        sb.AppendLine($"⚡ *Signal*");
        sb.AppendLine($"Action: *{evaluation.Action}* | Position: *{evaluation.PositionSideAfter}*");
        sb.AppendLine();

        sb.AppendLine($"💰 *Execution Details*");
        sb.AppendLine(
            $"Execution: *{TelegramValueFormatter.NullableDecimal(evaluation.ExecutionPrice)}* | Stop: *{TelegramValueFormatter.NullableDecimal(evaluation.ActiveStop)}*");

        if (summary.LongProfitPercent is not null)
        {
            var profitEmoji = summary.LongProfitPercent.Value >= 0 ? "🟢" : "🔴";
            sb.AppendLine($"{profitEmoji} Profit: *{TelegramValueFormatter.Percent(summary.LongProfitPercent.Value)}*");
        }

        sb.AppendLine();
        sb.AppendLine($"📝 *Reason*");
        sb.AppendLine(evaluation.Reason);

        return sb.ToString();
    }
}
