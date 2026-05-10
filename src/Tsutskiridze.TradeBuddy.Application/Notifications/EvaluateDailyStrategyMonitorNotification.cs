using SharedKernel;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring;

namespace Tsutskiridze.TradeBuddy.Application.Notifications;

public sealed record EvaluateDailyStrategyMonitorNotification(
    StrategyMonitorEvaluationSummary Summary) : IBaseNotification;
