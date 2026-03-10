using SharedKernel;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Events;
using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;

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
        PriceDirection direction) : base(id)
    {
        if (Guid.Empty == id) throw new DomainException("Invalid id");
        if (Guid.Empty == chatId) throw new DomainException("Invalid chatId");
        if (stockId == Guid.Empty) throw new DomainException("Invalid stockId");
        if (price < 0) throw new DomainException("Invalid price");

        ChatId = chatId;
        StockId = stockId;
        Price = price;
        Direction = direction;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PriceAlertCreatedEvent(this));
    }

    private PriceAlert()
    {
    }

    public bool IsTriggered(decimal currentPrice)
    {
        return Direction == PriceDirection.Above
            ? currentPrice >= Price
            : currentPrice <= Price;
    }

    public bool IsRateLimited(DateTime nowUtc, TimeSpan window)
        => UpdatedAt.HasValue && UpdatedAt.Value >= nowUtc.Subtract(window);

    public void RecordNotification()
    {
        if (!IsActive)
            throw new DomainException("Alert is Inactive");

        AlertCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasReachedMaxNotifications(int max)
    {
        return AlertCount >= max;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("Alert is already active");

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Alert is already inactive");

        IsActive = false;
        RaiseDomainEvent(new PriceAlertRemovedEvent(this));
    }
}