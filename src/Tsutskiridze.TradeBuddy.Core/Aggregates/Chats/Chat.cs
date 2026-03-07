using SharedKernel;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Core.Events;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;

public class Chat : Entity<Guid>, IAggregateRoot
{
    public long? TelegramChatId { get; private set; }
    public string? PrivateName { get; private set; }
    public string? ActivationToken { get; private set; }

    private Chat(){}
    
    public void Activate(long telegramChatId)
    {
        if (TelegramChatId.HasValue)
        {
            throw new DomainException("Chat is already activated");
        }

        TelegramChatId = telegramChatId;
    }

    public bool IsActivated()
    {
        return TelegramChatId.HasValue;
    }
}