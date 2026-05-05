using System.Globalization;
using Mediator;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.API.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.API.Telegram.Errors;
using Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Commands.CreateStrategy;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.Enums;

namespace Tsutskiridze.TradeBuddy.API.Telegram.Commands;

public sealed class CreateStrategyTelegramCommandHandler : ITelegramCommandHandler
{
    private static string Usage => $"Usage:\n{TelegramCommandCatalog.AddStrategy.Usage}";

    private readonly IMediator _mediator;

    public CreateStrategyTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Command => TelegramCommandCatalog.AddStrategy.Command;
    public string Description => TelegramCommandCatalog.AddStrategy.Description;

    public async Task<TelegramCommandDispatchResponse> Handle(
        TelegramCommandDispatchRequest dispatchRequest,
        CancellationToken ct)
    {
        if (dispatchRequest.Args.Count != 12)
            throw new TelegramPresentationException(Usage);
        var args = dispatchRequest.Args;

        RequireKeyword(args[0], "ema");
        var emaFastPeriod = ParseInt(args[1], "EMA fast period");
        var emaSlowPeriod = ParseInt(args[2], "EMA slow period");

        RequireKeyword(args[3], "adx");
        var adxPeriod = ParseInt(args[4], "ADX period");
        var adxTrendStrengthThreshold = ParseDecimal(args[5], "ADX trend strength threshold");
        var adxNonFallingLookBackBars = ParseInt(args[6], "ADX non-falling lookback bars");

        RequireKeyword(args[7], "atr");
        var atrPeriod = ParseInt(args[8], "ATR period");
        var atrInitialStopMultiplier = ParseDecimal(args[9], "ATR initial stop multiplier");
        var atrTrailingStopMultiplier = ParseDecimal(args[10], "ATR trailing stop multiplier");
        var atrTrailingActivationMultiplier = ParseInt(args[11], "ATR trailing activation multiplier");

        var result = await _mediator.Send(
            new CreateStrategyCommand(
                dispatchRequest.ChatId,
                Timeframe.Daily,
                emaFastPeriod,
                emaSlowPeriod,
                adxPeriod,
                adxTrendStrengthThreshold,
                adxNonFallingLookBackBars,
                atrPeriod,
                atrInitialStopMultiplier,
                atrTrailingStopMultiplier,
                atrTrailingActivationMultiplier),
            ct);

        return TelegramCommandDispatchResponse.TextReply(
            dispatchRequest.ChatId,
            CreateMessage(
                result.StrategyId,
                Timeframe.Daily,
                emaFastPeriod,
                emaSlowPeriod,
                adxPeriod,
                adxTrendStrengthThreshold,
                adxNonFallingLookBackBars,
                atrPeriod,
                atrInitialStopMultiplier,
                atrTrailingStopMultiplier,
                atrTrailingActivationMultiplier),
            ParseMode.Markdown
        );
    }

    private static void RequireKeyword(string value, string keyword)
    {
        if (!value.Equals(keyword, StringComparison.OrdinalIgnoreCase))
            throw new TelegramPresentationException(Usage);
    }

    private static int ParseInt(string value, string fieldName)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            throw new TelegramPresentationException($"{fieldName} must be a whole number.");

        return result;
    }

    private static decimal ParseDecimal(string value, string fieldName)
    {
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
            throw new TelegramPresentationException($"{fieldName} must be a valid decimal number.");

        return result;
    }

    private static string CreateMessage(
        string strategyId,
        Timeframe timeframe,
        int emaFastPeriod,
        int emaSlowPeriod,
        int adxPeriod,
        decimal adxTrendStrengthThreshold,
        int adxNonFallingLookBackBars,
        int atrPeriod,
        decimal atrInitialStopMultiplier,
        decimal atrTrailingStopMultiplier,
        int atrTrailingActivationMultiplier)
    {
        return $"{strategyId} : {timeframe} " +
               $"EMA {emaFastPeriod}/{emaSlowPeriod}, " +
               $"ADX {adxPeriod}/{adxTrendStrengthThreshold}/{adxNonFallingLookBackBars}, " +
               $"ATR {atrPeriod}/{atrInitialStopMultiplier}/{atrTrailingStopMultiplier}/{atrTrailingActivationMultiplier}.";
    }
}
