using SharedKernel.Data;
using SharedKernel.Events;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Application.Notifications;
using Tsutskiridze.TradeBuddy.Domain.Chats;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts.Events;
using Tsutskiridze.TradeBuddy.Domain.Stocks;

namespace Tsutskiridze.TradeBuddy.Application.EventHandlers.PriceAlerts;

public sealed class PriceAlertTriggeredDomainEventHandler
    : IDomainEventHandler<PriceAlertTriggeredDomainEvent>
{
    private readonly IReadRepository<Chat> _chats;
    private readonly IReadRepository<Stock> _stocks;
    private readonly INotificationDispatcher _notifier;
    public PriceAlertTriggeredDomainEventHandler(
        IReadRepository<Chat> chats,
        IReadRepository<Stock> stocks,
        INotificationDispatcher notifier)
    {
        _chats = chats;
        _stocks = stocks;
        _notifier = notifier;
    }
    public async ValueTask Handle(PriceAlertTriggeredDomainEvent domainEvent, CancellationToken ct)
    {
        var chat = await _chats.FirstOrDefaultAsync(new ActiveChatTelegramIdsByIdsSpec([domainEvent.ChatId]), ct);
        if (chat?.TelegramId is null)
            return;
        
        var stock = await _stocks.FirstOrDefaultAsync(
            new StockSymbolByIdSpec(domainEvent.StockId), ct
        );
        if (stock is null)
            return;
        
        await _notifier.DispatchAsync(new PriceAlertTriggeredNotification(
            chat.TelegramId,
            stock.Symbol,
            stock.Currency,
            domainEvent.CurrentPrice,
            domainEvent.AlertPrice,
            domainEvent.Direction,
            domainEvent.WasDeactivated,
            domainEvent.MaxNotifications), ct);
    }
}