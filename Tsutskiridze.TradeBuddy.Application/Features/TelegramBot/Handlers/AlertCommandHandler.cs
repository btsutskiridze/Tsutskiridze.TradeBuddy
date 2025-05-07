using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Database;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Yahoo;
using Tsutskiridze.TradeBuddy.Core.Constants;
using Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram;
using Tsutskiridze.TradeBuddy.Core.Enums;
using Tsutskiridze.TradeBuddy.Core.Helpers;


namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers
{
    public class AlertCommandHandler : ITelegramCommandHandler
    {
        public string Command => TelegramCommands.Alert;

        public string Description => "Set a price alert: /alert <symbol> <above|below> <price>";

        private readonly IYahooStockScraper _scraper;
        private readonly ITelegramBotClient _telegramClient;
        private readonly IServiceScopeFactory _scopes;

        public AlertCommandHandler(
          IYahooStockScraper scraper,
          ITelegramBotClient telegramClient,
          IServiceScopeFactory scopes)
        {
            _scraper = scraper;
            _telegramClient = telegramClient;
            _scopes = scopes;
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

            if (chat == null)
            {
                await _telegramClient.SendMessage(message.Chat.Id,
                    "sorry, you are not active user");
                return;
            }

            await _telegramClient.SendChatAction(message.Chat.Id, ChatAction.Typing);

            if (stock == null)
            {
                stock = new Stock
                {
                    Currency = quote.Currency,
                    Symbol = symbol,
                    Name = quote.Name,
                    IsWatched = true
                };

                db.Set<Stock>().Add(stock);
            }
            else
            {
                stock.IsWatched = true;
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

            var alertDirection = direction == PriceAlertDirection.Above ? "above" : "below";

            await _telegramClient.SendMessage(message.Chat.Id,
                $"✅ Price alert set for {symbol} {alertDirection} {CurrencyHelper.GetCurrencySymbol(quote.Currency)}{price}"
            );
        }
    }
}
