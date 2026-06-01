using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;

public interface IEmaAdxAtrEvaluationCandleBuilder
{
    IReadOnlyList<EmaAdxAtrEvaluationCandle> BuildDailyStrategyCandles(
        int fastEmaPeriod,
        int slowEmaPeriod,
        int adxPeriod,
        int atrPeriod,
        IReadOnlyList<MarketCandle> candles);
}