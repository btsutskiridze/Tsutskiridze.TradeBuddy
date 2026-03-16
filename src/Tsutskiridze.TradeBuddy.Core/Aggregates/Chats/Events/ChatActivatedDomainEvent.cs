using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Events;

public sealed record ChatActivatedDomainEvent(Guid ChatId, long TelegramChatId) : DomainEvent;