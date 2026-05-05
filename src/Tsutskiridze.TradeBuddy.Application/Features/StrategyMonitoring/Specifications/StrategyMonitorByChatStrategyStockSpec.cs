using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring;

namespace Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Specifications;

public sealed class StrategyMonitorByChatStrategyStockSpec : Specification<StrategyMonitor, int>
{
    public StrategyMonitorByChatStrategyStockSpec(Guid chatId, int strategyId, Guid stockId)
    {
        Query.Where(x =>
            x.ChatId == chatId && x.TradeStrategyId == strategyId && x.StockId == stockId
        );
    }
}