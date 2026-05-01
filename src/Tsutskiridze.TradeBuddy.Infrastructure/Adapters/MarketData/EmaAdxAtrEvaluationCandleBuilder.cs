using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Adapters.MarketData;

//todo: rename relocate and reorganize overal this folder
public class EmaAdxAtrEvaluationCandleBuilder : IEmaAdxAtrEvaluationCandleBuilder
{
    public IReadOnlyList<EmaAdxAtrEvaluationCandle> BuildDailyStrategyCandles(
        int fastEmaPeriod,
        int slowEmaPeriod,
        int adxPeriod,
        int atrPeriod,
        IReadOnlyList<MarketCandle> candles)
    {
        if (candles.Count == 0)
            return Array.Empty<EmaAdxAtrEvaluationCandle>();

        var orderedCandles = candles
            .OrderBy(x => x.Date)
            .ToArray();

        var closes = orderedCandles
            .Select(x => x.Close)
            .ToArray();

        var slowEma = CalculateEma(closes, fastEmaPeriod);
        var fastEma = CalculateEma(closes, slowEmaPeriod);
        var atr = CalculateAtr(orderedCandles, atrPeriod);
        var adx = CalculateAdx(orderedCandles, adxPeriod);

        var result = new EmaAdxAtrEvaluationCandle[orderedCandles.Length];

        for (var i = 0; i < orderedCandles.Length; i++)
        {
            var candle = orderedCandles[i];

            result[i] = new EmaAdxAtrEvaluationCandle(
                Date: candle.Date,
                Close: candle.Close,
                Low: candle.Low,
                SlowEma: slowEma[i],
                FastEma: fastEma[i],
                Atr: atr[i],
                Adx: adx[i]);
        }

        return result
            .Where(x
                => x is { FastEma: not null, SlowEma: not null, Atr: not null, Adx: not null }
            ).ToArray();
    }

    private static decimal?[] CalculateEma(
        IReadOnlyList<decimal> values,
        int period)
    {
        var result = new decimal?[values.Count];

        if (values.Count < period)
            return result;

        var multiplier = 2m / (period + 1);

        var initialSma = values
            .Take(period)
            .Average();

        var previousEma = initialSma;
        result[period - 1] = previousEma;

        for (var i = period; i < values.Count; i++)
        {
            var currentEma = ((values[i] - previousEma) * multiplier) + previousEma;

            result[i] = currentEma;
            previousEma = currentEma;
        }

        return result;
    }

    private static decimal?[] CalculateAtr(
        IReadOnlyList<MarketCandle> candles,
        int period)
    {
        var result = new decimal?[candles.Count];

        if (candles.Count < period)
            return result;

        var trueRanges = CalculateTrueRanges(candles);

        var initialAtr = trueRanges
            .Take(period)
            .Average();

        result[period - 1] = initialAtr;

        var previousAtr = initialAtr;

        for (var i = period; i < candles.Count; i++)
        {
            var currentAtr = ((previousAtr * (period - 1)) + trueRanges[i]) / period;

            result[i] = currentAtr;
            previousAtr = currentAtr;
        }

        return result;
    }

    private static decimal?[] CalculateAdx(
        IReadOnlyList<MarketCandle> candles,
        int period)
    {
        var result = new decimal?[candles.Count];

        // Need enough candles for:
        // 1. Initial smoothed TR/+DM/-DM
        // 2. Initial ADX average from DX values
        if (candles.Count < (period * 2))
            return result;

        var trueRanges = CalculateTrueRanges(candles);
        var plusDm = new decimal[candles.Count];
        var minusDm = new decimal[candles.Count];
        var dx = new decimal?[candles.Count];

        for (var i = 1; i < candles.Count; i++)
        {
            var upMove = candles[i].High - candles[i - 1].High;
            var downMove = candles[i - 1].Low - candles[i].Low;

            plusDm[i] = upMove > downMove && upMove > 0m
                ? upMove
                : 0m;

            minusDm[i] = downMove > upMove && downMove > 0m
                ? downMove
                : 0m;
        }

        var smoothedTrueRange = trueRanges
            .Skip(1)
            .Take(period)
            .Sum();

        var smoothedPlusDm = plusDm
            .Skip(1)
            .Take(period)
            .Sum();

        var smoothedMinusDm = minusDm
            .Skip(1)
            .Take(period)
            .Sum();

        var firstDxIndex = period;

        dx[firstDxIndex] = CalculateDx(
            smoothedTrueRange,
            smoothedPlusDm,
            smoothedMinusDm);

        for (var i = period + 1; i < candles.Count; i++)
        {
            smoothedTrueRange =
                smoothedTrueRange - (smoothedTrueRange / period) + trueRanges[i];

            smoothedPlusDm =
                smoothedPlusDm - (smoothedPlusDm / period) + plusDm[i];

            smoothedMinusDm =
                smoothedMinusDm - (smoothedMinusDm / period) + minusDm[i];

            dx[i] = CalculateDx(
                smoothedTrueRange,
                smoothedPlusDm,
                smoothedMinusDm);
        }

        var firstAdxIndex = (period * 2) - 1;

        var initialDxValues = dx
            .Skip(firstDxIndex)
            .Take(period)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToArray();

        if (initialDxValues.Length < period)
            return result;

        var previousAdx = initialDxValues.Average();
        result[firstAdxIndex] = previousAdx;

        for (var i = firstAdxIndex + 1; i < candles.Count; i++)
        {
            if (dx[i] is null)
                continue;

            var currentAdx = ((previousAdx * (period - 1)) + dx[i]!.Value) / period;

            result[i] = currentAdx;
            previousAdx = currentAdx;
        }

        return result;
    }

    private static decimal[] CalculateTrueRanges(
        IReadOnlyList<MarketCandle> candles)
    {
        var result = new decimal[candles.Count];

        if (candles.Count == 0)
            return result;

        result[0] = candles[0].High - candles[0].Low;

        for (var i = 1; i < candles.Count; i++)
        {
            var highLow = candles[i].High - candles[i].Low;
            var highPreviousClose = Math.Abs(candles[i].High - candles[i - 1].Close);
            var lowPreviousClose = Math.Abs(candles[i].Low - candles[i - 1].Close);

            result[i] = Math.Max(
                highLow,
                Math.Max(highPreviousClose, lowPreviousClose));
        }

        return result;
    }

    private static decimal CalculateDx(
        decimal smoothedTrueRange,
        decimal smoothedPlusDm,
        decimal smoothedMinusDm)
    {
        if (smoothedTrueRange == 0m)
            return 0m;

        var plusDi = 100m * (smoothedPlusDm / smoothedTrueRange);
        var minusDi = 100m * (smoothedMinusDm / smoothedTrueRange);

        var diSum = plusDi + minusDi;

        if (diSum == 0m)
            return 0m;

        return 100m * (Math.Abs(plusDi - minusDi) / diSum);
    }

    private static decimal? GetValueOrNull(
        IReadOnlyList<decimal?> values,
        int index)
    {
        if (index < 0 || index >= values.Count)
            return null;

        return values[index];
    }
}