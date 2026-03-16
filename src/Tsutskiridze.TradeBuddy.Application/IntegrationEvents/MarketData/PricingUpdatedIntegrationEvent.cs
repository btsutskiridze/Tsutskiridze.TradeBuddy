using SharedKernel.Events;

namespace Tsutskiridze.TradeBuddy.Application.IntegrationEvents.MarketData
{
    public record PricingUpdatedIntegrationEvent(string Symbol, decimal Price) : IIntegrationEvent;
}
