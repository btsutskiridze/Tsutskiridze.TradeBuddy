namespace Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;

public enum StrategyAction
{
    None = 0,

    StayOut = 1,
    EnterLong = 2,
    HoldLong = 3,
    ExitLongByStop = 4,
    ExitLongByEmaCross = 5,

    InsufficientData = 100
}