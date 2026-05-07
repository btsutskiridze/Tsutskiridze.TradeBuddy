using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Notifications;

public sealed record EvaluateDailyStrategyMonitorNotification(    
    long ChatId,
    string StrategyCode,
    string Symbol,
    StrategyAction Action,
    PositionSide PositionSideAfter,
    DateOnly CandleDate,
    decimal ClosePrice,
    decimal? ExecutionPrice,
    decimal? ActiveStop,
    decimal? LongProfitPercent,
    bool ShouldNotify,
    string Reason) :IBaseNotification;