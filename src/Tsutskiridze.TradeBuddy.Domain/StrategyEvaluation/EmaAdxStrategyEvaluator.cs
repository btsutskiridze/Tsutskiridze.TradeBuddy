using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Enums;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.ValueObjects;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.ValueObjects;

namespace Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;

public sealed class EmaAdxAtrStrategyEvaluator
{
    public static StrategyEvaluationResult EvaluateLatest(
        TradeStrategy tradeStrategy,
        StrategyMonitor monitor,
        IReadOnlyList<EmaAdxAtrEvaluationCandle> candles)
    {
        ValidateInputs(
            tradeStrategy.EmaTrend,
            tradeStrategy.AdxTrendStrength,
            tradeStrategy.AtrStop,
            monitor.PositionState,
            candles);

        var orderedCandles = candles
            .OrderBy(x => x.Date)
            .ToArray();

        var todayIndex = orderedCandles.Length - 1;

        return EvaluateAt(
            tradeStrategy.EmaTrend,
            tradeStrategy.AdxTrendStrength,
            tradeStrategy.AtrStop,
            monitor.PositionState,
            orderedCandles,
            todayIndex);
    }

    public static IReadOnlyList<StrategyEvaluationResult> Replay(
        TradeStrategy tradeStrategy,
        StrategyMonitor monitor,
        IReadOnlyList<EmaAdxAtrEvaluationCandle> candles)
    {
        ValidateInputs(
            tradeStrategy.EmaTrend,
            tradeStrategy.AdxTrendStrength,
            tradeStrategy.AtrStop,
            monitor.PositionState,
            candles);

        var orderedCandles = candles
            .OrderBy(x => x.Date)
            .ToArray();

        var results = new List<StrategyEvaluationResult>(orderedCandles.Length);

        for (var i = 0; i < orderedCandles.Length; i++)
        {
            var result = EvaluateAt(
                tradeStrategy.EmaTrend,
                tradeStrategy.AdxTrendStrength,
                tradeStrategy.AtrStop,
                monitor.PositionState,
                orderedCandles,
                i);

            results.Add(result);
        }

        return results;
    }

    private static StrategyEvaluationResult EvaluateAt(
        EmaTrendSettings emaSettings,
        AdxTrendStrengthSettings adxSettings,
        AtrStopSettings atrSettings,
        StrategyPositionState positionState,
        EmaAdxAtrEvaluationCandle[] candles,
        int index)
    {
        var today = candles[index];

        return positionState.Side switch
        {
            PositionSide.OutOfMarket => EvaluateOutOfMarket(
                adxSettings,
                atrSettings,
                positionState,
                candles,
                index,
                today),

            PositionSide.Long => EvaluateLong(
                atrSettings,
                positionState,
                today),

            _ => throw new DomainException("Unsupported position side.")
        };
    }

    private static StrategyEvaluationResult EvaluateOutOfMarket(
        AdxTrendStrengthSettings adxSettings,
        AtrStopSettings atrSettings,
        StrategyPositionState positionState,
        IReadOnlyList<EmaAdxAtrEvaluationCandle> candles,
        int index,
        EmaAdxAtrEvaluationCandle today)
    {
        if (!HasRequiredEntryIndicators(today))
        {
            return InsufficientData(
                today,
                positionState,
                "Cannot evaluate entry because today's EMA, ADX, or ATR value is missing.");
        }

        var setupToday = IsEntrySetupAt(candles, index, adxSettings);

        var setupYesterday = index > 0
                              && IsEntrySetupAt(candles, index - 1, adxSettings);

        var isNewSetup = setupToday && !setupYesterday;

        if (!isNewSetup)
        {
            return new StrategyEvaluationResult(
                Action: StrategyAction.StayOut,
                PositionSideAfter: PositionSide.OutOfMarket,
                CandleDate: today.Date,
                ClosePrice: today.Close,
                EntryPrice: null,
                ExecutionPrice: null,
                ActiveStop: null,
                ShouldNotify: false,
                Reason: setupToday
                    ? "Entry setup exists, but it is not new today."
                    : "Entry setup is not active today.");
        }

        var lockedAtr = today.Atr!.Value;

        var initialStop =
            today.Close - lockedAtr * atrSettings.InitialStopMultiplier;

        positionState.EnterLong(
            entryPrice: today.Close,
            lockedAtr: lockedAtr,
            entryDate: DateTime.SpecifyKind(today.Date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc),
            initialStop: initialStop);

        return new StrategyEvaluationResult(
            Action: StrategyAction.EnterLong,
            PositionSideAfter: PositionSide.Long,
            CandleDate: today.Date,
            ClosePrice: today.Close,
            EntryPrice: today.Close,
            ExecutionPrice: today.Close,
            ActiveStop: initialStop,
            ShouldNotify: true,
            Reason: "New long setup appeared today: fast EMA is above slow EMA, ADX is strong, and ADX is not falling.");
    }

    private static StrategyEvaluationResult EvaluateLong(
        AtrStopSettings atrSettings,
        StrategyPositionState positionState,
        EmaAdxAtrEvaluationCandle today)
    {
        if (positionState.ActiveStop is null)
            throw new DomainException("Active stop is required while position is long.");

        if (positionState.EntryPrice is null)
            throw new DomainException("Entry price is required while position is long.");

        if (positionState.LockedAtr is null)
            throw new DomainException("Locked ATR is required while position is long.");

        var activeStopBeforeEvaluation = positionState.ActiveStop.Value;
        var entryPrice = positionState.EntryPrice;
        // Conservative daily-candle rule:
        // If today's low touched active stop, exit first.
        if (today.Low <= activeStopBeforeEvaluation)
        {
            positionState.Exit();

            return new StrategyEvaluationResult(
                Action: StrategyAction.ExitLongByStop,
                PositionSideAfter: PositionSide.OutOfMarket,
                CandleDate: today.Date,
                ClosePrice: today.Close,
                EntryPrice: entryPrice,
                ExecutionPrice: activeStopBeforeEvaluation,
                ActiveStop: null,
                ShouldNotify: true,
                Reason: "Today's low touched the active stop.");
        }

        if (today.FastEma is null || today.SlowEma is null)
        {
            return InsufficientData(
                today,
                positionState,
                "Cannot evaluate EMA cross exit because today's EMA value is missing.");
        }

        if (today.FastEma.Value <= today.SlowEma.Value)
        {
            positionState.Exit();

            return new StrategyEvaluationResult(
                Action: StrategyAction.ExitLongByEmaCross,
                PositionSideAfter: PositionSide.OutOfMarket,
                CandleDate: today.Date,
                ClosePrice: today.Close,
                EntryPrice:entryPrice,
                ExecutionPrice: today.Close,
                ActiveStop: null,
                ShouldNotify: true,
                Reason: "Fast EMA crossed below or equal to slow EMA.");
        }

        positionState.UpdateHighestClose(today.Close);

        var activationPrice =
            positionState.EntryPrice.Value
            + positionState.LockedAtr.Value * atrSettings.TrailingActivationMultiplier;

        if (!positionState.TrailingActivated && today.Close >= activationPrice)
        {
            positionState.ActivateTrailing();
        }

        if (positionState.TrailingActivated)
        {
            var newStop =
                positionState.HighestClose!.Value
                - positionState.LockedAtr.Value * atrSettings.TrailingStopMultiplier;

            positionState.RatchetStop(newStop);
        }

        return new StrategyEvaluationResult(
            Action: StrategyAction.HoldLong,
            PositionSideAfter: PositionSide.Long,
            CandleDate: today.Date,
            ClosePrice: today.Close,
            EntryPrice:entryPrice,
            ExecutionPrice: null,
            ActiveStop: positionState.ActiveStop,
            ShouldNotify: false,
            Reason: "Long position is still active.");
    }

    private static bool IsEntrySetupAt(
        IReadOnlyList<EmaAdxAtrEvaluationCandle> candles,
        int index,
        AdxTrendStrengthSettings adxSettings)
    {
        var candle = candles[index];

        if (!HasRequiredEntryIndicators(candle))
            return false;

        var emaTrendIsBullish =
            candle.FastEma!.Value > candle.SlowEma!.Value;

        var adxIsStrong =
            candle.Adx!.Value >= adxSettings.TrendStrengthThreshold;

        var adxIsNotFalling =
            IsAdxNotFalling(candles, index, adxSettings.NonFallingLookBackBars);

        return emaTrendIsBullish && adxIsStrong && adxIsNotFalling;
    }

    private static bool IsAdxNotFalling(
        IReadOnlyList<EmaAdxAtrEvaluationCandle> candles,
        int index,
        int previousBarsToCheck)
    {
        if (previousBarsToCheck <= 0)
            throw new DomainException("ADX lookback bars must be positive.");

        // Example:
        // previousBarsToCheck = 3 means:
        // today >= yesterday >= two days ago >= three days ago
        if (index - previousBarsToCheck < 0)
            return false;

        for (var i = index; i > index - previousBarsToCheck; i--)
        {
            var currentAdx = candles[i].Adx;
            var previousAdx = candles[i - 1].Adx;

            if (currentAdx is null || previousAdx is null)
                return false;

            if (currentAdx.Value < previousAdx.Value)
                return false;
        }

        return true;
    }

    private static bool HasRequiredEntryIndicators(EmaAdxAtrEvaluationCandle candle)
    {
        return candle.FastEma is not null
               && candle.SlowEma is not null
               && candle.Atr is not null
               && candle.Adx is not null;
    }

    private static StrategyEvaluationResult InsufficientData(
        EmaAdxAtrEvaluationCandle candle,
        StrategyPositionState positionState,
        string reason)
    {
        return new StrategyEvaluationResult(
            Action: StrategyAction.InsufficientData,
            PositionSideAfter: positionState.Side,
            CandleDate: candle.Date,
            ClosePrice: candle.Close,
            EntryPrice:null,
            ExecutionPrice: null,
            ActiveStop: positionState.ActiveStop,
            ShouldNotify: false,
            Reason: reason);
    }

    private static void ValidateInputs(
        EmaTrendSettings emaSettings,
        AdxTrendStrengthSettings adxSettings,
        AtrStopSettings atrSettings,
        StrategyPositionState positionState,
        IReadOnlyList<EmaAdxAtrEvaluationCandle> candles)
    {
        if (emaSettings is null)
            throw new DomainException("EMA settings are required.");

        if (adxSettings is null)
            throw new DomainException("ADX settings are required.");

        if (atrSettings is null)
            throw new DomainException("ATR settings are required.");

        if (positionState is null)
            throw new DomainException("Position state is required.");

        if (candles is null || candles.Count == 0)
            throw new DomainException("At least one candle is required.");
    }
}