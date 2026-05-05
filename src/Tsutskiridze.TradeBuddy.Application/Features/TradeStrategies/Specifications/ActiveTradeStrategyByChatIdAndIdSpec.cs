using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;

namespace Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Specifications;

internal sealed class ActiveTradeStrategyByChatIdAndIdSpec : Specification<TradeStrategy, int>
{
    public ActiveTradeStrategyByChatIdAndIdSpec(Guid chatId, int strategyId)
    {
        Query.Where(strategy =>
            strategy.IsActive &&
            strategy.ChatId == chatId &&
            strategy.Id == strategyId);
    }
}
