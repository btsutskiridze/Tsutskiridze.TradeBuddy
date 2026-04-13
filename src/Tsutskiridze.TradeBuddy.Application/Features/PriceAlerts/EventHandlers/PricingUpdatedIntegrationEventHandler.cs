using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.Events;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Utilities;
using Tsutskiridze.TradeBuddy.Application.IntegrationEvents.MarketData;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.EventHandlers;

//todo: fix this implementation
public sealed class PricingUpdatedIntegrationEventHandler : IIntegrationEventHandler<PricingUpdatedIntegrationEvent>
{
    private readonly IUnitOfWork _uow;
    private readonly IReadRepository<Chat> _chats;
    private readonly IRepository<Stock> _stocks;
    private readonly IRepository<PriceAlert> _alerts;
    private readonly ITelegramBotClient _bot;
    private readonly ICurrencySymbolProvider _currency;
    private readonly ILogger<PricingUpdatedIntegrationEventHandler> _log;
    private readonly IDbExceptionClassifier _excClassifier;


    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromSeconds(10);
    private const int MaxNotifications = 5;


    public PricingUpdatedIntegrationEventHandler(
        IUnitOfWork uow,
        IReadRepository<Chat> chats,
        IRepository<Stock> stocks,
        IRepository<PriceAlert> alerts,
        ITelegramBotClient bot,
        ICurrencySymbolProvider currency,
        ILogger<PricingUpdatedIntegrationEventHandler> log,
        IDbExceptionClassifier excClassifier)
    {
        _uow = uow;
        _chats = chats;
        _stocks = stocks;
        _alerts = alerts;
        _bot = bot;
        _currency = currency;
        _log = log;
        _excClassifier = excClassifier;
    }

    public async ValueTask Handle(PricingUpdatedIntegrationEvent evt, CancellationToken ct)
    {
        try
        {
            var stock = await _stocks.FirstOrDefaultAsync(
                new StockBySymbolSpec(evt.Symbol),
                ct: ct
            );

            if (stock is null)
                return;

            var activeAlerts = await _alerts.ListAsync(
                new ActiveAlertsByStockIdSpec(stock.Id),
                ct
            );

            if (activeAlerts.Count == 0)
                return;

            var triggeredAlerts = activeAlerts
                .Where(x => x.IsTriggered(evt.Price) && !x.IsRateLimited(DateTime.UtcNow, RateLimitWindow))
                .ToList();

            if (triggeredAlerts.Count == 0)
                return;

            var chatIds = triggeredAlerts
                .Select(x => x.ChatId)
                .Distinct()
                .ToList();

            var chatsDict = (await _chats.ListAsync(
                new ActivatedChatsByIdsSpec(chatIds),
                ct: ct)).ToDictionary(x => x.Id);

            var currencySymbol = _currency.GetSymbol(stock.Currency) ?? stock.Currency;

            var outgoingMessages = new List<OutgoingTelegramMessage>();

            foreach (var alert in triggeredAlerts.TakeWhile(_ => !ct.IsCancellationRequested))
            {
                if (!chatsDict.TryGetValue(alert.ChatId, out var chat))
                    continue;

                var dirEmoji = alert.Direction == PriceDirection.Above ? "🚀" : "📉";
                var dirText = alert.Direction == PriceDirection.Above ? "Above" : "Below";

                outgoingMessages.Add(new OutgoingTelegramMessage(
                    chat.TelegramChatId!.Value,
                    $"🔔 *{evt.Symbol}*: {currencySymbol}{evt.Price:N2} 🔔\n{dirEmoji} {dirText} {currencySymbol}{alert.Price:N2}"
                ));

                alert.RecordNotification();
                if (!alert.HasReachedMaxNotifications(MaxNotifications)) continue;

                alert.Deactivate();
                outgoingMessages.Add(new OutgoingTelegramMessage(
                    chat.TelegramChatId.Value,
                    $"🚫 *{evt.Symbol}* Alert removed after {MaxNotifications} notifications."
                ));
            }

            // After in-memory mutations, decide stock watch state from the tracked alert list.
            if (stock.IsWatched && !activeAlerts.Any(x => x.IsActive))
            {
                stock.UnWatch();
            }

            await _uow.SaveChangesAsync(ct);

            await Task.WhenAll(outgoingMessages.Select(x => SendTelegramMessage(x, ct)));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _log.LogWarning(ex,
                "Concurrency conflict while processing pricing update for {Symbol}",
                evt.Symbol);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error in {Handler} for {Symbol}", nameof(PricingUpdatedIntegrationEventHandler),
                evt.Symbol);
        }
    }

    private async Task SendTelegramMessage(OutgoingTelegramMessage message, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return;

        await _bot.SendMessage(
            message.ChatId,
            message.Text,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private sealed record OutgoingTelegramMessage(long ChatId, string Text);
}