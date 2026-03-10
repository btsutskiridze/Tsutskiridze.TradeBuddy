using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Events;

public sealed record ChatActivatedEvent(Guid ChatId, long TelegramChatId) : DomainEvent;