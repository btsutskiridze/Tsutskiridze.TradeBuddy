using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.Chats.Events;

public sealed record ChatActivatedDomainEvent(Guid ChatId, long TelegramChatId) : DomainEvent;