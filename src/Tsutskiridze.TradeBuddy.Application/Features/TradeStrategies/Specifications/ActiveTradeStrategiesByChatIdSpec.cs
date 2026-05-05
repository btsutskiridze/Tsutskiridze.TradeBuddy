using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;

namespace Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Specifications;


internal sealed class ActiveTradeStrategiesByChatIdSpec : Specification<TradeStrategy, int>
{
    public ActiveTradeStrategiesByChatIdSpec(Guid chatId, int? strategyId)
    {
        Query.Where(strategy => strategy.IsActive && strategy.ChatId == chatId);

        if (strategyId.HasValue)
        {
            Query.Where(strategy => strategy.Id == strategyId.Value);
        }

        Query.OrderBy(strategy => strategy.Id);
    }
}