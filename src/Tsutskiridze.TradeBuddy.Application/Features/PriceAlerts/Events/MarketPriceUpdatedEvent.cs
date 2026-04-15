using SharedKernel.Events;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Events
{
    public record MarketPriceUpdatedEvent(string Symbol, decimal Price) : IApplicationEvent;
}
