using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Specifications;

public sealed class AlertByStockAndChatSpec : Specification<PriceAlert>
{
    public AlertByStockAndChatSpec(
        Guid chatId,
        Guid stockId,
        PriceAlertDirection direction,
        decimal price)
    {
        Query.Where(x =>
            x.ChatId == chatId && x.StockId == stockId
                               && x.Price == price && x.Direction == direction
        );
    }
}