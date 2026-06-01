namespace Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;

public sealed record EmaAdxAtrEvaluationCandle(
    DateOnly Date,
    decimal Close,
    decimal Low,
    decimal? SlowEma,
    decimal? FastEma,
    decimal? Atr,
    decimal? Adx);