using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Specifications;

public sealed class ActiveAlertsByChatIdSpec : Specification<PriceAlert>
{
    public ActiveAlertsByChatIdSpec(Guid chatId)
    {
        Query.Where(x => x.IsActive && x.ChatId == chatId);
    }
}