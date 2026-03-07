using Mediator;

namespace Tsutskiridze.TradeBuddy.Application.Events
{
    public record PricingUpdatedEvent(string Symbol, decimal Price) : INotification;
}
