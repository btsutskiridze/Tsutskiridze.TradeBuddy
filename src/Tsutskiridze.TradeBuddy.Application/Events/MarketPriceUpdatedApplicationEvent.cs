using SharedKernel.Events;

namespace Tsutskiridze.TradeBuddy.Application.Events
{
    public record MarketPriceUpdatedApplicationEvent(string Symbol, decimal Price) : IApplicationEvent;
}
