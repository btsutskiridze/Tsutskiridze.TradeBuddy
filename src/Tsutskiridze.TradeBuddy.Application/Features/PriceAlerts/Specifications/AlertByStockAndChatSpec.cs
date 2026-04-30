using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Enums;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Specifications;

public sealed class AlertByStockAndChatSpec : Specification<PriceAlert>
{
    public AlertByStockAndChatSpec(
        Guid chatId,
        Guid stockId,
        PriceDirection direction,
        decimal price)
    {
        Query.Where(x =>
            x.ChatId == chatId && x.StockId == stockId
                               && x.Trigger.Price == price && x.Trigger.Direction == direction
        );
    }
}