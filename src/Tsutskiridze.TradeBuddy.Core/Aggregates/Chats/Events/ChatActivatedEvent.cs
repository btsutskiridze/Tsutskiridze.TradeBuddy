using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Events;

public sealed class ChatActivatedEvent(Guid ChatId, long TelegramChatId) : DomainEvent;