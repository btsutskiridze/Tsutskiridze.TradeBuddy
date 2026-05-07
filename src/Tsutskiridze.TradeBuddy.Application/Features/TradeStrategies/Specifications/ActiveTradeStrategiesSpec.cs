using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;

namespace Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Specifications;

public sealed class ActiveTradeStrategiesSpec: Specification<TradeStrategy, int>
{
    public ActiveTradeStrategiesSpec()
    {
        Query.Where(x => x.IsActive);
    }
}