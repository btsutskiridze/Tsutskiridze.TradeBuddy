using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;

namespace Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Specifications;

internal sealed class TradeStrategyByChatIdAndIdSpec : Specification<TradeStrategy, int>
{
    public TradeStrategyByChatIdAndIdSpec(Guid chatId, int strategyId)
    {
        Query.Where(strategy =>
            strategy.ChatId == chatId &&
            strategy.Id == strategyId);
    }
}
