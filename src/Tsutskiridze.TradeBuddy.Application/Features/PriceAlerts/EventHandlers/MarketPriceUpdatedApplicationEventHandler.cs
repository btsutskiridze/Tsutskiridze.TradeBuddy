using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.Events;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Events;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Notifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks.Specifications;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.EventHandlers;

public sealed class MarketPriceUpdatedApplicationEventHandler : IApplicationEventHandler<MarketPriceUpdatedEvent>
{
    private readonly IUnitOfWork _uow;
    private readonly IReadRepository<Chat> _chats;
    private readonly IRepository<Stock> _stocks;
    private readonly IRepository<PriceAlert> _alerts;
    private readonly INotificationDispatcher _notifier;
    private readonly ILogger<MarketPriceUpdatedApplicationEventHandler> _log;

    private static readonly TimeSpan AlertWindow = TimeSpan.FromSeconds(10);
    private const int MaxNotifications = 5;


    public MarketPriceUpdatedApplicationEventHandler(
        IUnitOfWork uow,
        IReadRepository<Chat> chats,
        IRepository<Stock> stocks,
        IRepository<PriceAlert> alerts,
        ILogger<MarketPriceUpdatedApplicationEventHandler> log,
        INotificationDispatcher notifier)
    {
        _uow = uow;
        _chats = chats;
        _stocks = stocks;
        _alerts = alerts;
        _log = log;
        _notifier = notifier;
    }

    public async ValueTask Handle(MarketPriceUpdatedEvent evt, CancellationToken ct)
    {
        var stock = await _stocks.FirstOrDefaultAsync(new StockBySymbolSpec(evt.Symbol), ct);
        if (stock is null)
            return;

        var activeAlerts = await _alerts.ListAsync(new ActiveAlertsByStockIdSpec(stock.Id), ct);
        if (activeAlerts.Count == 0)
            return;

        var chatIds = activeAlerts
            .Select(x => x.ChatId)
            .Distinct()
            .ToList();

        var chats = (await _chats.ListAsync(new ActivatedChatsByIdsSpec(chatIds), ct))
            .ToDictionary(x => x.Id, x => x.TelegramChatId!.Value);

        var notifications = new List<PriceAlertTriggeredNotification>();

        foreach (var alert in activeAlerts)
        {
            if (ct.IsCancellationRequested)
                break;

            if (!chats.TryGetValue(alert.ChatId, out var telegramChatId))
                continue;

            var result = alert.ProcessMarketPrice(
                evt.Price,
                DateTime.UtcNow,
                AlertWindow,
                MaxNotifications);

            if (result is null)
                continue;

            notifications.Add(new PriceAlertTriggeredNotification(
                telegramChatId,
                stock.Symbol,
                stock.Currency,
                result.CurrentPrice,
                result.AlertPrice,
                result.Direction,
                result.WasDeactivated,
                MaxNotifications));
        }

        await _uow.SaveChangesAsync(ct);
        
        await _notifier.DispatchAsync(notifications, ct);

        _log.LogInformation($"Processed market price update for {evt.Symbol}. Notifications sent: {notifications.Count}");
    }
}