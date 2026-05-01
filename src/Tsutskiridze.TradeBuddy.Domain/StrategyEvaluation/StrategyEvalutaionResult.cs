using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Enums;

namespace Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;

public sealed record StrategyEvaluationResult(
    StrategyAction Action,
    PositionSide PositionSideAfter,
    DateOnly CandleDate,
    decimal ClosePrice,
    decimal? ExecutionPrice,
    decimal? ActiveStop,
    bool ShouldNotify,
    string Reason)
{
    public bool IsEntryOrExit =>
        Action is StrategyAction.EnterLong
            or StrategyAction.ExitLongByStop
            or StrategyAction.ExitLongByEmaCross;
}