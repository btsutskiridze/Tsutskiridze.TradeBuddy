using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;

namespace Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Events;

public sealed record StrategyMonitorAlertDomainEvent(
    int StrategyMonitorId,
    Guid ChatId,
    int TradeStrategyId,
    string Symbol,
    StrategyEvaluationResult Evaluation) : DomainEvent
{
    public override string EventType => "strategy_monitor.alert.v1";
}
