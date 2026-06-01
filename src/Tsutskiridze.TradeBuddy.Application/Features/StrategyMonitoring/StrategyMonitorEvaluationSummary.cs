using Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;

namespace Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring;

public sealed record StrategyMonitorEvaluationSummary(
    long ChatId,
    string StrategyCode,
    string Symbol,
    StrategyEvaluationResult Evaluation,
    decimal? LongProfitPercent)
{
    public static StrategyMonitorEvaluationSummary Create(
        long chatId,
        string strategyCode,
        string symbol,
        StrategyEvaluationResult evaluation)
    {
        return new StrategyMonitorEvaluationSummary(
            chatId,
            strategyCode,
            symbol,
            evaluation,
            CalculateLongProfitPercent(evaluation));
    }

    private static decimal? CalculateLongProfitPercent(StrategyEvaluationResult evaluation)
    {
        return evaluation.Action is
            StrategyAction.HoldLong or StrategyAction.ExitLongByEmaCross or StrategyAction.ExitLongByStop
            && evaluation.EntryPrice is not null
            ? (((evaluation.ExecutionPrice ?? evaluation.ClosePrice) - evaluation.EntryPrice.Value) *
               100) / evaluation.EntryPrice.Value
            : null;
    }
}
