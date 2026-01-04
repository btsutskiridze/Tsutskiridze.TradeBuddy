using Mediator;

namespace Tsutskiridze.TradeBuddy.Application.Events
{
    public record PricingUpdated(string Symbol, decimal Price) : INotification;
}
