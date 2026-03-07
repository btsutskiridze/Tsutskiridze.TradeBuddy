using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Specifications;

public sealed class ActiveAlertsByChatIdSpec : Specification<PriceAlert>
{
    public ActiveAlertsByChatIdSpec(Guid chatId)
    {
        Query.Where(x => x.IsActive && x.ChatId == chatId);
    }
}