using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Specifications;


public sealed class DuplicateAlertSpec : Specification<PriceAlert>
{
    public DuplicateAlertSpec(Guid chatId, Guid stockId, PriceDirection direction, decimal price)
    {
        Query.Where(x => x.ChatId == chatId && x.StockId == stockId && x.Trigger.Direction == direction && x.Trigger.Price == price);
    }
}