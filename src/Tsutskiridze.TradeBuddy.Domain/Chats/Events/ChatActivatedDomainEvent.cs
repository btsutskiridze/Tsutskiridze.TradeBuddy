using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.Chats.Events;

public sealed record ChatActivatedDomainEvent(Guid ChatId, long TelegramChatId) : DomainEvent
{
    public override string EventType => "chat.activated.v1";
}