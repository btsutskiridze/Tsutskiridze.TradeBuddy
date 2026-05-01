using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts.Enums;

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