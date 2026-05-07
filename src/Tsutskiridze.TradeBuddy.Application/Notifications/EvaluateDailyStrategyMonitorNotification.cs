using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Notifications;

public sealed record EvaluateDailyStrategyMonitorNotification(    
    StrategyAction Action,
    PositionSide PositionSideAfter,
    DateOnly CandleDate,
    decimal ClosePrice,
    decimal? ExecutionPrice,
    decimal? ActiveStop,
    decimal? LongProfitPercent,
    bool ShouldNotify,
    string Reason) :IBaseNotification;