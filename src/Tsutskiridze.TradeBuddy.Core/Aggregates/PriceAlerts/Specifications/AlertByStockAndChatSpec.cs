using SharedKernel;
using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Specifications;

public class AlertByStockAndChatSpec : Specification<PriceAlert>
{
    public AlertByStockAndChatSpec(
        Guid chatId, 
        Guid stockId, 
        PriceAlertDirection direction,
        decimal price)
    {
        Query(query => query.Where(x => 
            x.ChatId == chatId && x.StockId == stockId
                               && x.Price == price && x.Direction == direction
        ));
    }
}