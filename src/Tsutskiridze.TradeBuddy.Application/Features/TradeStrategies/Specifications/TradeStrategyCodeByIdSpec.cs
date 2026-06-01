using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.ValueObjects;

namespace Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Specifications;

internal sealed class TradeStrategyCodeByIdSpec : Specification<TradeStrategy, int, TradeStrategyCode>
{
    public TradeStrategyCodeByIdSpec(int strategyId)
    {
        Query
            .Select(strategy => strategy.Code)
            .Where(strategy => strategy.Id == strategyId);
    }
}