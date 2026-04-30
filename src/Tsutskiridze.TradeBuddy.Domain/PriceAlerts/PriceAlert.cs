using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.Enums;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts.ValueObjects;

namespace Tsutskiridze.TradeBuddy.Domain.PriceAlerts;

public class PriceAlert : Entity<Guid>, IAggregateRoot
{
    public Guid ChatId { get; private init; }
    public Guid StockId { get; private init; }
    public AlertTrigger Trigger { get; private init; }
    public int AlertCount { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }


    public PriceAlert(
        Guid id,
        Guid chatId,
        Guid stockId,
        AlertTrigger trigger,
        DateTime nowUtc) : base(id)
    {
        if (Guid.Empty == id) throw new DomainException("Invalid id");
        if (Guid.Empty == chatId) throw new DomainException("Invalid chatId");
        if (stockId == Guid.Empty) throw new DomainException("Invalid stockId");

        ChatId = chatId;
        StockId = stockId;
        Trigger = trigger;
        CreatedAt = nowUtc;
    }

    private PriceAlert()
    {
    }
    
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        AlertCount = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public PriceAlertProcessingResult? ProcessMarketPrice(
        PriceTick priceTick, 
        AlertTriggerPolicy policy
    )
    {
        if (!IsActive)
            return null;

        if (!Trigger.IsTriggeredBy(priceTick.Price))
            return null;

        if (IsCoolingDown(priceTick.Timestamp, policy.CooldownWindow))
            return null;

        TriggerNotification(priceTick.Timestamp);

        var wasDeactivated = DeactivateAfterNotificationLimit(policy);

        return new PriceAlertProcessingResult(
            ChatId,
            StockId,
            Trigger.Price,
            priceTick.Price,
            Trigger.Direction,
            wasDeactivated
        );
    }

    private bool DeactivateAfterNotificationLimit(AlertTriggerPolicy policy)
    {
        if (AlertCount < policy.MaxNotifications)
            return false;
        
        Deactivate();
        return true;
    }

    private void TriggerNotification(DateTime nowUtc)
    {
        AlertCount++;
        UpdatedAt = nowUtc;
    }

    private bool IsCoolingDown(DateTime nowUtc, TimeSpan window)
        => AlertCount != 0 && UpdatedAt.HasValue && UpdatedAt.Value >= nowUtc.Subtract(window);

    public sealed record PriceAlertProcessingResult(
        Guid ChatId,
        Guid StockId,
        decimal AlertPrice,
        decimal CurrentPrice,
        PriceDirection Direction,
        bool WasDeactivated);
}