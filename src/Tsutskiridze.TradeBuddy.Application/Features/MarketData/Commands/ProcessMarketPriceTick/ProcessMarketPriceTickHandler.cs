using Mediator;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Application.Notifications;
using Tsutskiridze.TradeBuddy.Domain.AlertWatching;
using Tsutskiridze.TradeBuddy.Domain.Chats;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts.ValueObjects;
using Tsutskiridze.TradeBuddy.Domain.Stocks;

namespace Tsutskiridze.TradeBuddy.Application.Features.MarketData.Commands.ProcessMarketPriceTick;

public record ProcessMarketPriceTickCommand(string Symbol, decimal Price, DateTime OccurredAtUtc) : ICommand;

public sealed class ProcessMarketPriceTickHandler : ICommandHandler<ProcessMarketPriceTickCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly IReadRepository<Chat> _chats;
    private readonly IRepository<Stock> _stocks;
    private readonly IRepository<PriceAlert> _alerts;
    private readonly AlertWatchingDomainService _alertWatchingSvc;
    private readonly INotificationDispatcher _notifier;
    private readonly ILogger<ProcessMarketPriceTickHandler> _log;

    private static readonly AlertTriggerPolicy AlertTriggerPolicy = new(
        TimeSpan.FromSeconds(10),
        5
    );

    public ProcessMarketPriceTickHandler(
        IUnitOfWork uow,
        IReadRepository<Chat> chats,
        IRepository<Stock> stocks,
        IRepository<PriceAlert> alerts,
        ILogger<ProcessMarketPriceTickHandler> log,
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

    public async ValueTask<Unit> Handle(ProcessMarketPriceTickCommand command, CancellationToken ct)
    {
        var stock = await _stocks.FirstOrDefaultAsync(new StockBySymbolSpec(command.Symbol), ct);

        if (stock is null)
            return Unit.Value;

        var activeAlerts = await _alerts.ListAsync(new ActiveAlertsByStockIdSpec(stock.Id), ct);

        if (activeAlerts.Count == 0)
            return Unit.Value;

        var triggerResults = ProcessAlertsAgainstTick(command, activeAlerts, ct);

        _alertWatchingSvc.EnsureStockWatchState(stock, activeAlerts.Any(x => x.IsActive));

        var notifications = await BuildTriggeredAlertNotificationsAsync(stock, triggerResults, ct);

        await _uow.SaveChangesAsync(ct);

        await _notifier.DispatchAsync(notifications, ct);

        _log.LogInformation(
            "Processed market price update for {Symbol}. Notifications sent: {NotificationCount}",
            command.Symbol,
            notifications.Count
        );
        
        return Unit.Value;
    }

    private static List<PriceAlert.PriceAlertProcessingResult> ProcessAlertsAgainstTick(
        ProcessMarketPriceTickCommand command, List<PriceAlert> activeAlerts, CancellationToken ct)
    {
        var tick = new PriceTick(command.Price, command.OccurredAtUtc);

        var triggerResults = new List<PriceAlert.PriceAlertProcessingResult>();

        foreach (var alert in activeAlerts)
        {
            if (ct.IsCancellationRequested)
                break;

            var result = alert.ProcessMarketPrice(
                tick,
                AlertTriggerPolicy
            );

            if (result is not null)
                triggerResults.Add(result);
        }

        return triggerResults;
    }

    private async ValueTask<List<PriceAlertTriggeredNotification>> BuildTriggeredAlertNotificationsAsync(Stock stock,
        List<PriceAlert.PriceAlertProcessingResult> triggerResults, CancellationToken ct = default)
    {
        var chatIds = triggerResults
            .Select(x => x.ChatId)
            .Distinct()
            .ToArray();

        var chats = (await _chats.ListAsync(new ActiveChatsByIdsSpec(chatIds), ct))
            .ToDictionary(x => x.Id, x => x.TelegramChatId!.Value);

        var notifications = new List<PriceAlertTriggeredNotification>();

        foreach (var item in triggerResults)
        {
            if (!chats.TryGetValue(item.ChatId, out var telegramChatId))
                continue;

            notifications.Add(new PriceAlertTriggeredNotification(
                telegramChatId,
                stock.Symbol,
                stock.Currency,
                item.CurrentPrice,
                item.AlertPrice,
                item.Direction,
                item.WasDeactivated,
                AlertTriggerPolicy.MaxNotifications
            ));
        }

        return notifications;
    }
}