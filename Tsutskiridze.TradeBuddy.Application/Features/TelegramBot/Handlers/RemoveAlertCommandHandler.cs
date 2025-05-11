using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Database;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Helpers;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Yahoo;
using Tsutskiridze.TradeBuddy.Core.Constants;
using Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram;
using Tsutskiridze.TradeBuddy.Core.Enums;


namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers
{
    public class RemoveAlertCommandHandler : ITelegramCommandHandler
    {
        public string Command => TelegramCommands.RemoveAlert;

        public string Description => "<symbol> <above|below> <price>";

        private readonly IYahooStockScraper _scraper;
        private readonly ITelegramBotClient _telegramClient;
        private readonly IServiceScopeFactory _scopes;
        private readonly IMediator _publisher;
        private readonly ICurrencySymbolProvider _currency;
        public RemoveAlertCommandHandler(
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
                    $"e.g: /{Command} NVDA above 300");
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

            var alert = db.Set<PriceAlert>()
                .Where(x => x.Chat.TelegramChatID == message.Chat.Id &&
                             x.Stock.Symbol == symbol &&
                             x.Direction == direction &&
                             x.Price == price)
                .FirstOrDefault();

            if (alert == null)
            {
                await _telegramClient.SendMessage(message.Chat.Id,
                 $"No alert found for '{symbol}' with direction '{direction}' and price '{price}'.");
                return;
            }

            db.Set<PriceAlert>().Remove(alert);
            await db.SaveChangesAsync();

            await _telegramClient.SendMessage(message.Chat.Id, "" +
                $"Alert for '{symbol}' with direction '{direction}' and price '{price}' has been removed.");
        }
    }
}
