using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.Enums;

namespace Tsutskiridze.TradeBuddy.Domain.PriceAlerts.ValueObjects;

public record AlertTrigger: ValueObject
{
    public PriceDirection Direction { get; init; }
    public decimal Price { get; init; }

    private AlertTrigger(PriceDirection direction, decimal price)
    {
        if(!Enum.IsDefined(direction))
            throw new DomainException("Invalid alert direction.");    
        
        if(price <= 0)
            throw new DomainException("Invalid alert price.");   
        
        Direction = direction;
        Price = price;   
    }

    public static AlertTrigger Create(PriceDirection direction, decimal price)
    {
        return new AlertTrigger(direction, price);
    }
    
    public bool IsTriggeredBy(decimal currentPrice)
    {
        return Direction == PriceDirection.Above
            ? currentPrice >= Price
            : currentPrice <= Price;
    }
}