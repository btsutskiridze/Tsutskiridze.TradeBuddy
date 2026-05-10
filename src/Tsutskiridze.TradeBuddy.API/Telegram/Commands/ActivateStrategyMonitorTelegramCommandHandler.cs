using System.Globalization;
using Mediator;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.API.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.API.Telegram.Errors;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Commands.ActivateStrategyMonitor;

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
            CreateMessage(result),
            ParseMode.Markdown);
    }

    private static string CreateMessage(StrategyMonitorEvaluationSummary result)
    {
        var evaluation = result.Evaluation;

        return
            $"Strategy monitor activated for `{result.Symbol}` with `{result.StrategyCode}`.\n" +
            $"Latest candle: `{evaluation.CandleDate:yyyy-MM-dd}` close `{FormatDecimal(evaluation.ClosePrice)}`.\n" +
            (result.LongProfitPercent is null ? "" : $"Profit: {result.LongProfitPercent.Value:N}%\n") +
            $"Action: `{evaluation.Action}`. Position: `{evaluation.PositionSideAfter}`.\n" +
            $"Execution: `{FormatNullableDecimal(evaluation.ExecutionPrice)}`. Stop: `{FormatNullableDecimal(evaluation.ActiveStop)}`.\n" +
            $"Reason: {evaluation.Reason}";
    }

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