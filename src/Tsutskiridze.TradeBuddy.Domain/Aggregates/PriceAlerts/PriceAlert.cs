using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.Events;
using Tsutskiridze.TradeBuddy.Domain.Enums;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts;

public class PriceAlert : Entity<Guid>, IAggregateRoot
{
    public Guid ChatId { get; private init; }
    public Guid StockId { get; private init; }

    public decimal Price { get; private init; }
    public PriceDirection Direction { get; private init; }

    public int AlertCount { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }


    public PriceAlert(
        Guid id,
        Guid chatId,
        Guid stockId,
        decimal price,
        PriceDirection direction,
        DateTime nowUtc) : base(id)
    {
        if (Guid.Empty == id) throw new DomainException("Invalid id");
        if (Guid.Empty == chatId) throw new DomainException("Invalid chatId");
        if (stockId == Guid.Empty) throw new DomainException("Invalid stockId");
        if (price < 0) throw new DomainException("Invalid price");

        ChatId = chatId;
        StockId = stockId;
        Price = price;
        Direction = direction;
        CreatedAt = nowUtc;
    }

    private PriceAlert()
    {
    }

    public void RecordNotification()
    {
        if (!IsActive)
            throw new DomainException("Alert is Inactive");

        AlertCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        AlertCount = 0;
        RaiseDomainEvent(new PriceAlertActivatedDomainEvent(StockId));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        RaiseDomainEvent(new PriceAlertDeactivatedDomainEvent(StockId));
    }

    public PriceAlertProcessingResult? ProcessMarketPrice(
        decimal currentPrice,
        DateTime nowUtc,
        TimeSpan window,
        int maxNotifications
    )
    {
        if (!IsActive)
            return null;

        if (!IsTriggered(currentPrice))
            return null;

        if (IsRateLimited(nowUtc, window))
            return null;

        AlertCount++;
        UpdatedAt = nowUtc;

        var wasDeactivated = false;
        if (AlertCount >= maxNotifications)
        {
            Deactivate();
            wasDeactivated = true;
        }

        return new PriceAlertProcessingResult(
            ChatId,
            StockId,
            Price,
            currentPrice,
            Direction,
            wasDeactivated
        );
    }

    private bool IsTriggered(decimal currentPrice)
    {
        return Direction == PriceDirection.Above
            ? currentPrice >= Price
            : currentPrice <= Price;
    }

    private bool IsRateLimited(DateTime nowUtc, TimeSpan window)
        => UpdatedAt.HasValue && UpdatedAt.Value >= nowUtc.Subtract(window);

    public sealed record PriceAlertProcessingResult(
        Guid ChatId,
        Guid StockId,
        decimal AlertPrice,
        decimal CurrentPrice,
        PriceDirection Direction,
        bool WasDeactivated);
}