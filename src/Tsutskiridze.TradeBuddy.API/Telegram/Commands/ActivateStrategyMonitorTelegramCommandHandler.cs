using System.Text;
using Mediator;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.API.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.API.Telegram.Errors;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Commands.ActivateStrategyMonitor;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram.Formatting;

namespace Tsutskiridze.TradeBuddy.API.Telegram.Commands;

public sealed class ActivateStrategyMonitorTelegramCommandHandler : ITelegramCommandHandler
{
    private static string Usage => $"Usage:\n{TelegramCommandCatalog.MonitorStrategy.Usage}";

    private readonly IMediator _mediator;

    public ActivateStrategyMonitorTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Command => TelegramCommandCatalog.MonitorStrategy.Command;
    public string Description => TelegramCommandCatalog.MonitorStrategy.Description;

    public async Task<TelegramCommandDispatchResponse> Handle(
        TelegramCommandDispatchRequest dispatchRequest,
        CancellationToken ct)
    {
        if (dispatchRequest.Args.Count != 2)
        {
            throw new TelegramPresentationException(Usage);
        }

        var symbol = dispatchRequest.Args[0];
        var strategyCode = dispatchRequest.Args[1];

        if (string.IsNullOrWhiteSpace(strategyCode) || string.IsNullOrWhiteSpace(symbol))
        {
            throw new TelegramPresentationException(Usage);
        }

        var result = await _mediator.Send(
            new ActivateStrategyMonitorCommand(dispatchRequest.ChatId, strategyCode, symbol),
            ct);

        return TelegramCommandDispatchResponse.TextReply(
            dispatchRequest.ChatId,
            StrategyMonitorTelegramMessageFormatter.CreateActivationMessage(result),
            ParseMode.Markdown);
    }
    
    public static string CreateActivationMessage(StrategyMonitorEvaluationSummary summary)
    {
        var evaluation = summary.Evaluation;
        var sb = new StringBuilder();

        sb.AppendLine($"✅ *Strategy Monitor Activated*");
        sb.AppendLine($"Symbol: *{summary.Symbol}* | Strategy: *{summary.StrategyCode}*");
        sb.AppendLine();

        sb.AppendLine($"🕯 *Latest Candle*");
        sb.AppendLine(
            $"Date: *{evaluation.CandleDate:yyyy-MM-dd}* | Close: *{TelegramValueFormatter.Decimal(evaluation.ClosePrice)}*");
        sb.AppendLine();

        sb.AppendLine($"⚡ *Current Signal*");
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