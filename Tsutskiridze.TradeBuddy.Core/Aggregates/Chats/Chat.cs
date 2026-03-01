using SharedKernel;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Core.Events;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;

public class Chat : Entity<Guid>, IAggregateRoot
{
    private List<PriceAlert> _priceAlerts = [];
    
    public long? TelegramChatId { get; private set; }
    public string? PrivateName { get; private set; }
    public string? ActivationToken { get; private set; }
    public IReadOnlyList<PriceAlert> PriceAlerts => _priceAlerts.AsReadOnly();

    private Chat(){}
    
    public void Activate(long telegramChatId)
    {
        if (TelegramChatId.HasValue)
        {
            throw new DomainException("Chat is already activated");
        }

        TelegramChatId = telegramChatId;
    }

    public void AddPriceAlert(PriceAlert priceAlert)
    {
        ArgumentNullException.ThrowIfNull(priceAlert);
        _priceAlerts.Add(priceAlert);
        RaiseDomainEvent(new PriceAlertCreatedEvent(priceAlert));
    }

    public void RemovePriceAlert(PriceAlert priceAlert)
    {
        ArgumentNullException.ThrowIfNull(priceAlert);
        _priceAlerts.Remove(priceAlert);
        RaiseDomainEvent(new PriceAlertRemovedEvent(priceAlert));
    }
}