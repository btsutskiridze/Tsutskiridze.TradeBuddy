using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.Enums;

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
                               && x.Price == price && x.Direction == direction
        );
    }
}