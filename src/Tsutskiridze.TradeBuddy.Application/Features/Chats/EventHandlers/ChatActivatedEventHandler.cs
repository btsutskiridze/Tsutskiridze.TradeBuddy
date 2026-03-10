using Mediator;
using SharedKernel;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Events;

namespace Tsutskiridze.TradeBuddy.Application.Features.Chats.EventHandlers;

public sealed class ChatActivatedEventHandler : INotificationHandler<ChatActivatedEvent>
{
    private readonly ITelegramSender _sender;

    public ChatActivatedEventHandler(ITelegramSender sender)
    {
        _sender = sender;
    }

    public async ValueTask Handle(ChatActivatedEvent notification, CancellationToken cancellationToken)
    {
        await _sender.SendMessage(notification.TelegramChatId, "StockBuddy Activated Successfully", cancellationToken);
    }
}