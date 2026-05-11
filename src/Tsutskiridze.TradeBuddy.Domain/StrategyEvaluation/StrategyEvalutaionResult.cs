using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Enums;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.ValueObjects;

namespace Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;

public sealed record StrategyEvaluationResult(
    StrategyAction Action,
    PositionSide PositionSideAfter,
    DateOnly CandleDate,
    decimal ClosePrice,
    decimal? EntryPrice,
    decimal? ExecutionPrice,
    decimal? ActiveStop,
    bool ShouldNotify,
    string Reason,
    StrategyPositionState PositionStateAfter
    )
{
    public bool IsEntryOrExit =>
        Action is StrategyAction.EnterLong
            or StrategyAction.ExitLongByStop
            or StrategyAction.ExitLongByEmaCross;
}