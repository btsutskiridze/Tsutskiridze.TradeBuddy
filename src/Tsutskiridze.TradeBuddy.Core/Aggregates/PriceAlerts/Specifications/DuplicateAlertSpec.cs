using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Specifications;


public sealed class DuplicateAlertSpec : Specification<PriceAlert>
{
    public DuplicateAlertSpec(Guid chatId, Guid stockId, PriceAlertDirection direction, decimal price)
    {
        Query.Where(x => x.ChatId == chatId && x.StockId == stockId && x.Direction == direction && x.Price == price);
    }
}