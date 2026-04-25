using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats.Events;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats;

public class Chat : Entity<Guid>, IAggregateRoot
{
    public long? TelegramChatId { get; private set; }
    public string? PrivateName { get; private set; }
    public string? ActivationToken { get; private set; }

    private Chat()
    {
    }
    
    /*
     *todo:
     *Introduce StockSymbol, Money,
     *CurrencyCode, ActivationToken, TelegramChatId,
     *and richer aggregate factories/methods.
     * 
     */

    public void Activate(long telegramChatId)
    {
        if (TelegramChatId.HasValue)
        {
            throw new DomainException("Chat is already activated");
        }

        TelegramChatId = telegramChatId;
        RaiseDomainEvent(new ChatActivatedDomainEvent(this.Id, TelegramChatId.Value));
    }

    public void EnsureActivated()
    {
        if (!TelegramChatId.HasValue)
            throw new DomainException("Chat is not activated");
    }
}