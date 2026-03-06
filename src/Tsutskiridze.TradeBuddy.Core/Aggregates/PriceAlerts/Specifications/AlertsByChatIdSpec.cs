using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Specifications;

public sealed class AlertsByChatIdSpec : Specification<PriceAlert>
{
    public AlertsByChatIdSpec(Guid chatId)
    {
        Query.Where(x => x.IsActive && x.ChatId == chatId);
    }
}