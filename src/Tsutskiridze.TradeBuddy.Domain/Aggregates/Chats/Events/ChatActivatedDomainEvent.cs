using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats.Events;

public sealed record ChatActivatedDomainEvent(Guid ChatId, long TelegramChatId) : DomainEvent;