using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedKernel;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Helpers;
using Tsutskiridze.TradeBuddy.Application.Events;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Core.Enums;
namespace Tsutskiridze.TradeBuddy.Application.Features.StockAlerts;

public sealed class PriceChangeAlertService : INotificationHandler<PricingUpdated>
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ITelegramBotClient _bot;
    private readonly ICurrencySymbolProvider _currency;
    private readonly ILogger<PriceChangeAlertService> _log;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromSeconds(10);
    private const int MaxNotifications = 5;

    public PriceChangeAlertService(
        IServiceScopeFactory scopes,
        ITelegramBotClient bot,
        ILogger<PriceChangeAlertService> log,
        ICurrencySymbolProvider currency)
    {
        _scopes = scopes;
        _bot = bot;
        _log = log;
        _currency = currency;
    }

    public async ValueTask Handle(PricingUpdated evt, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);

        try
        {
            using var scope = _scopes.CreateScope();

            var stockRepository = scope.ServiceProvider.GetRequiredService<IRepository<Stock>>();
            var alertRepository = scope.ServiceProvider.GetRequiredService<IRepository<PriceAlert>>();
            var chatReadRepository = scope.ServiceProvider.GetRequiredService<IReadRepository<Chat>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var stock = await stockRepository.FirstOrDefaultAsync(
                x => x.Symbol == evt.Symbol,
                asNoTracking: false,
                ct: ct);

            if (stock is null)
                return;

            var nowUtc = DateTime.UtcNow;

            var activeAlertsForStock = await alertRepository.ListAsync(
                x => x.StockId == stock.Id && x.IsActive,
                asNoTracking: false,
                ct: ct);

            if (activeAlertsForStock.Count == 0)
                return;

            var triggeredAlerts = activeAlertsForStock
                .Where(x => x.IsTriggered(evt.Price) && !x.IsRateLimited(nowUtc, RateLimitWindow))
                .ToList();

            if (triggeredAlerts.Count == 0)
                return;

            var chatIds = triggeredAlerts
                .Select(x => x.ChatId)
                .Distinct()
                .ToList();

            var chats = await chatReadRepository.ListAsync(
                x => chatIds.Contains(x.Id),
                asNoTracking: true,
                ct: ct);

            var chatsById = chats.ToDictionary(x => x.Id);

            var currencySymbol = _currency.GetSymbol(stock.Currency) ?? stock.Currency;

            var outgoingMessages = new List<OutgoingTelegramMessage>();

            foreach (var alert in triggeredAlerts.TakeWhile(alert => !ct.IsCancellationRequested))
            {
                if (!chatsById.TryGetValue(alert.ChatId, out var chat))
                    continue;

                if (chat.TelegramChatId is null)
                    continue;

                var dirEmoji = alert.Direction == PriceAlertDirection.Above ? "🚀" : "📉";
                var dirText = alert.Direction == PriceAlertDirection.Above ? "Above" : "Below";

                outgoingMessages.Add(new OutgoingTelegramMessage(
                    chat.TelegramChatId.Value,
                    $"🔔 *{evt.Symbol}*: {currencySymbol}{evt.Price:N2} 🔔\n{dirEmoji} {dirText} {currencySymbol}{alert.Price:N2}"
                ));

                alert.RecordNotification();

                if (alert.HasReachedMaxNotifications(MaxNotifications))
                {
                    alert.Deactivate();

                    outgoingMessages.Add(new OutgoingTelegramMessage(
                        chat.TelegramChatId.Value,
                        $"🚫 *{evt.Symbol}* Alert removed after {MaxNotifications} notifications."
                    ));
                }
            }

            // After in-memory mutations, decide stock watch state from the tracked alert list.
            if (stock.IsWatched && activeAlertsForStock.All(x => !x.IsActive))
            {
                stock.UnWatch();
            }

            await unitOfWork.SaveChangesAsync(ct);

            var tasks = outgoingMessages.Select(x => SendTelegramMessage(x, ct));
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error in {Handler} for {Symbol}", nameof(PriceChangeAlertService), evt.Symbol);
        }
        finally
        {
            _lock.Release();
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