using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Events;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Database;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Helpers;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Yahoo;
using Tsutskiridze.TradeBuddy.Core.Constants;
using Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram;
using Tsutskiridze.TradeBuddy.Core.Enums;


namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers
{
    public class AlertCommandHandler : ITelegramCommandHandler
    {
        public string Command => TelegramCommands.Alert;

        public string Description => "/alert <symbol> <above|below> <price>";

        private readonly IYahooStockScraper _scraper;
        private readonly ITelegramBotClient _telegramClient;
        private readonly IServiceScopeFactory _scopes;
        private readonly IMediator _publisher;
        private readonly ICurrencySymbolProvider _currency;
        public AlertCommandHandler(
          IYahooStockScraper scraper,
          ITelegramBotClient telegramClient,
          IServiceScopeFactory scopes,
          IMediator publisher,
          ICurrencySymbolProvider currency)
        {
            _scraper = scraper;
            _telegramClient = telegramClient;
            _scopes = scopes;
            _publisher = publisher;
            _currency = currency;
        }

        public async Task HandleMessage(Message message)
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            var parts = message.Text!
                              .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 4 ||
                !Enum.TryParse<PriceAlertDirection>(
                   parts[2], true, out var direction) ||
                !decimal.TryParse(parts[3], out var price))
            {
                await _telegramClient.SendMessage(message.Chat.Id,
                    "Invalid command format. Use: /alert <symbol> <above|below> <price>");
                return;
            }

            await _telegramClient.SendChatAction(message.Chat.Id, ChatAction.Typing);

            var symbol = parts[1].ToUpperInvariant();
            if (!await _scraper.StockSymbolExits(symbol))
            {
                await _telegramClient.SendMessage(message.Chat.Id,
                    $"Stock symbol '{symbol}' not found.");
                return;
            }

            var quote = await _scraper.GetStockQuote(symbol);

            if (quote == null)
            {
                await _telegramClient.SendMessage(message.Chat.Id,
                    $"Failed to fetch quote for '{symbol}'.");
                return;
            }

            var chat = await db.Set<Core.DBEntities.Telegram.Chat>()
                .Where(x => x.TelegramChatID == message.Chat.Id)
                .FirstOrDefaultAsync();

            var stock = await db.Set<Stock>()
                .Where(x => x.Symbol == symbol)
                .FirstOrDefaultAsync();

            bool publishEvent = false;

            if (stock is null)
            {
                stock = new Stock
                {
                    Currency = quote.Currency,
                    Symbol = symbol,
                    Name = quote.Name,
                    IsWatched = true
                };

                db.Set<Stock>().Add(stock);
                publishEvent = true;
            }
            else if (stock.IsWatched == false)
            {
                stock.IsWatched = true;
                publishEvent = true;
            }

            var alert = new PriceAlert
            {
                Direction = direction,
                Price = price,
                Stock = stock,
                Chat = chat
            };

            db.Set<PriceAlert>().Add(alert);
            await db.SaveChangesAsync();

            if (publishEvent)
                await _publisher.Publish(new StockWatchStatusChanged(symbol, true));

            var alertDirection = direction == PriceAlertDirection.Above ? "above" : "below";

            var cur = _currency.GetSymbol(quote.Currency) ?? quote.Currency;

            await _telegramClient.SendMessage(message.Chat.Id,
                $"✅ Price alert set for {symbol} {alertDirection} {quote.Currency}{price}"
            );
        }
    }
}
