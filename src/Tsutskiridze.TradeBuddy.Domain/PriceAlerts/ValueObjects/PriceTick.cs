using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.PriceAlerts.ValueObjects;

public sealed record PriceTick : ValueObject
{
    public decimal Price { get; init; }
    public DateTime Timestamp { get; init; }

    public PriceTick(decimal price, DateTime timestamp)
    {
        if(price <= 0)
            throw new DomainException("Invalid price.");
        
        Price = price;
        Timestamp = timestamp;
    }
}