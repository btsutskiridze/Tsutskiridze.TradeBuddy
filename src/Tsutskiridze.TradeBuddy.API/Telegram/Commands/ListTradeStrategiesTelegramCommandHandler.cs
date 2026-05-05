using System.Globalization;
using System.Text;
using Mediator;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.API.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.API.Telegram.Errors;
using Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Queries.ListTradeStrategies;

namespace Tsutskiridze.TradeBuddy.API.Telegram.Commands;

public sealed class ListTradeStrategiesTelegramCommandHandler : ITelegramCommandHandler
{
    private static string Usage => $"Usage:\n{TelegramCommandCatalog.ListStrategies.Usage}";

    private readonly IMediator _mediator;

    public ListTradeStrategiesTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Command => TelegramCommandCatalog.ListStrategies.Command;
    public string Description => TelegramCommandCatalog.ListStrategies.Description;

    public async Task<TelegramCommandDispatchResponse> Handle(
        TelegramCommandDispatchRequest dispatchRequest,
        CancellationToken ct)
    {
        if (dispatchRequest.Args.Count > 1)
        {
            throw new TelegramPresentationException(Usage);
        }

        int? strategyId = dispatchRequest.Args.Count == 0
            ? null
            : ParseStrategyId(dispatchRequest.Args[0]);

        var result = await _mediator.Send(
            new ListTradeStrategiesCommand(dispatchRequest.ChatId, strategyId),
            ct);

        return TelegramCommandDispatchResponse.TextReply(
            dispatchRequest.ChatId,
            CreateMessage(result),
            ParseMode.Markdown);
    }

    private static int ParseStrategyId(string value)
    {
        var span = value.AsSpan();
        var separatorIndex = span.LastIndexOf('_');

        if (separatorIndex <= 0 || separatorIndex == span.Length - 1)
        {
            throw new TelegramPresentationException(Usage);
        }

        var prefix = span[..separatorIndex];
        if (!prefix.Equals("st", StringComparison.OrdinalIgnoreCase)
            && !prefix.Equals("ts", StringComparison.OrdinalIgnoreCase))
        {
            throw new TelegramPresentationException(Usage);
        }

        var idText = span[(separatorIndex + 1)..];
        if (!int.TryParse(idText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) || id <= 0)
        {
            throw new TelegramPresentationException(Usage);
        }

        return id;
    }

    private static string CreateMessage(ListTradeStrategiesResult result)
    {
        var lines = result.Strategies.Select((strategy, index) =>
            $"{index + 1}. `{strategy.Code}` {strategy.Timeframe}: " +
            $"EMA {strategy.EmaFastPeriod}/{strategy.EmaSlowPeriod}, " +
            $"ADX {strategy.AdxPeriod}/{FormatDecimal(strategy.AdxTrendStrengthThreshold)}/{strategy.AdxNonFallingLookBackBars}, " +
            $"ATR {strategy.AtrPeriod}/{FormatDecimal(strategy.AtrInitialStopMultiplier)}/" +
            $"{FormatDecimal(strategy.AtrTrailingStopMultiplier)}/{FormatDecimal(strategy.AtrTrailingActivationMultiplier)}");

        return new StringBuilder()
            .AppendLine("*Your Active Trade Strategies*")
            .AppendLine()
            .AppendJoin('\n', lines)
            .ToString();
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
