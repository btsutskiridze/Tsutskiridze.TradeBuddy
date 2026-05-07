using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Specifications;

public sealed class ActiveStrategyMonitorsByTradeStrategyIdsSpec : Specification<StrategyMonitor, int>
{

    public ActiveStrategyMonitorsByTradeStrategyIdsSpec(IReadOnlyCollection<int> tradeStrategyIds)
    {
        Query.Where(x => tradeStrategyIds.Contains(x.TradeStrategyId) && x.Status == MonitorStatus.Active);
    }
}