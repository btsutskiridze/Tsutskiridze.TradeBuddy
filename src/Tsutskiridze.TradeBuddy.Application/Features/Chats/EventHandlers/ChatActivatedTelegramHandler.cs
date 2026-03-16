using Mediator;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Events;

namespace Tsutskiridze.TradeBuddy.Application.Features.Chats.EventHandlers;

public sealed class ChatActivatedTelegramHandler : INotificationHandler<ChatActivatedDomainEvent>
{
    private readonly ITelegramSender _sender;

    public ChatActivatedTelegramHandler(ITelegramSender sender)
    {
        _sender = sender;
    }

    public async ValueTask Handle(ChatActivatedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _sender.SendMessage(notification.TelegramChatId, "StockBuddy Activated Successfully", cancellationToken);
    }
}