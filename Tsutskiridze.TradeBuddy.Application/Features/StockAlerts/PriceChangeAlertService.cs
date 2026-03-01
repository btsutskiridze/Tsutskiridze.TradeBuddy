using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedKernel;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Database;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Helpers;
using Tsutskiridze.TradeBuddy.Application.Events;
using Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram;
using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Features.StockAlerts
{
    public class PriceChangeAlertService : INotificationHandler<PricingUpdated>
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ITelegramBotClient _bot;
        private readonly ICurrencySymbolProvider _currency;
        private readonly ILogger<PriceChangeAlertService> _log;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private const int RateLimitSec = 10;
        private const int MaxCount = 5;

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
                var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                var threshold = DateTime.UtcNow.AddSeconds(-RateLimitSec);

                var alerts = await db.Set<PriceAlert>()
                    .Where(pa => pa.Stock.Symbol == evt.Symbol &&
                            ((pa.Direction == PriceAlertDirection.Above && evt.Price >= pa.Price) ||
                            (pa.Direction == PriceAlertDirection.Below && evt.Price <= pa.Price)) &&
                            (pa.UpdatedAt == null || pa.UpdatedAt < threshold))
                    .Include(pa => pa.Chat)
                    .Include(pa => pa.Stock)
                    .ToListAsync(ct);

                if (alerts.Count == 0) return;

                var symbol = evt.Symbol;
                var price = evt.Price;
                var cur = _currency.GetSymbol(alerts[0].Stock.Currency) ?? alerts[0].Stock.Currency;


                foreach (var alert in alerts)
                {
                    if (ct.IsCancellationRequested) break;
                    var dirEmoji = alert.Direction == PriceAlertDirection.Above ? "🚀" : "📉";
                    var dirText = alert.Direction == PriceAlertDirection.Above ? "Above" : "Below";

                    await _bot.SendMessage(
                        alert.Chat.TelegramChatID,
                        $"🔔 *{symbol}*: {cur}{price:N2} 🔔\n{dirEmoji} {dirText} {cur}{alert.Price:N2}",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: ct
                    );

                    alert.AlertCount++;
                    alert.UpdatedAt = DateTime.UtcNow;

                    if (alert.AlertCount >= MaxCount)
                    {
                        var otherExists = await db.Set<PriceAlert>()
                            .AnyAsync(pa => pa.StockID == alert.StockID && pa.ID != alert.ID, ct);

                        if (!otherExists)
                            alert.Stock.IsWatched = false;

                        db.Set<PriceAlert>().Remove(alert);
                        await _bot.SendMessage(
                            alert.Chat.TelegramChatID,
                            $"🚫 *{symbol}* Alert removed after {MaxCount} notifications.",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: ct
                        );
                    }
                }

                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error in AlertService for {Symbol}", evt.Symbol);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
