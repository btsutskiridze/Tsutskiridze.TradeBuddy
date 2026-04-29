using Microsoft.Extensions.Logging;
using SharedKernel.Data;
using SharedKernel.Events;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications;
using Tsutskiridze.TradeBuddy.Application.Events;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Application.Notifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.ValueObjects;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Domain.AlertWatching;

namespace Tsutskiridze.TradeBuddy.Application.EventHandlers.Stocks;

public sealed class
    MarketPriceUpdatedApplicationEventHandler : IApplicationEventHandler<MarketPriceUpdatedApplicationEvent>
{
    private readonly IUnitOfWork _uow;
    private readonly IReadRepository<Chat> _chats;
    private readonly IRepository<Stock> _stocks;
    private readonly IRepository<PriceAlert> _alerts;
    private readonly AlertWatchingDomainService _alertWatchingSvc;
    private readonly INotificationDispatcher _notifier;
    private readonly ILogger<MarketPriceUpdatedApplicationEventHandler> _log;

    private static readonly NotificationPolicy AlertNotificationPolicy = new(
        TimeSpan.FromSeconds(10),
        5
    );

    public MarketPriceUpdatedApplicationEventHandler(
        IUnitOfWork uow,
        IReadRepository<Chat> chats,
        IRepository<Stock> stocks,
        IRepository<PriceAlert> alerts,
        ILogger<MarketPriceUpdatedApplicationEventHandler> log,
        INotificationDispatcher notifier,
        AlertWatchingDomainService alertWatchingSvc
    )
    {
        _uow = uow;
        _chats = chats;
        _stocks = stocks;
        _alerts = alerts;
        _log = log;
        _notifier = notifier;
        _alertWatchingSvc = alertWatchingSvc;
    }

    public async ValueTask Handle(MarketPriceUpdatedApplicationEvent evt, CancellationToken ct)
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

        var chats = (await _chats.ListAsync(new ActiveChatsByIdsSpec(chatIds), ct))
            .ToDictionary(x => x.Id, x => x.TelegramChatId!.Value);


        var notifications = new List<PriceAlertTriggeredNotification>();

        foreach (var alert in activeAlerts)
        {
            if (ct.IsCancellationRequested)
                break;

            if (!chats.TryGetValue(alert.ChatId, out var telegramChatId))
                continue;

            var result = alert.ProcessMarketPrice(
                new PriceTick(evt.Price, DateTime.UtcNow), 
                AlertNotificationPolicy
            );

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
                AlertNotificationPolicy.MaxNotifications
            ));
        }

        _alertWatchingSvc.EnsureStockWatchState(stock, activeAlerts.Any(x => x.IsActive));

        await _uow.SaveChangesAsync(ct);

        await _notifier.DispatchAsync(notifications, ct);

        _log.LogInformation(
            $"Processed market price update for {evt.Symbol}. Notifications sent: {notifications.Count}");
    }
}