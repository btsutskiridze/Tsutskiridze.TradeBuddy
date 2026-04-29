using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.ValueObjects;

public sealed record NotificationPolicy : ValueObject
{
    public TimeSpan CooldownWindow { get; }
    public int MaxNotifications { get; }
    
    public NotificationPolicy(TimeSpan cooldownWindow, int maxNotifications)
    {
        if (cooldownWindow <= TimeSpan.Zero)
            throw new DomainException("Cooldown window must be positive.");

        if (maxNotifications <= 0)
            throw new DomainException("Max notifications must be positive.");

        CooldownWindow = cooldownWindow;
        MaxNotifications = maxNotifications;
    }
    
}