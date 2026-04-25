using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Enums;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.Specifications;


public sealed class DuplicateAlertSpec : Specification<PriceAlert>
{
    public DuplicateAlertSpec(Guid chatId, Guid stockId, PriceDirection direction, decimal price)
    {
        Query.Where(x => x.ChatId == chatId && x.StockId == stockId && x.Direction == direction && x.Price == price);
    }
}