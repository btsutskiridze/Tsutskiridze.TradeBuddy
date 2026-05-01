namespace Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;

public sealed record DailyStrategyCandle(
    DateOnly Date,
    decimal Close,
    decimal Low,
    decimal? FastEma,
    decimal? SlowEma,
    decimal? Atr,
    decimal? Adx);