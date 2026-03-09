using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Utilities;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Core.Enums;
using Chat = Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Chat;


namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers
{
    public class RemoveAlertCommandHandler : ITelegramCommandHandler
    {
        public string Command => TelegramCommands.RemoveAlert;
        public string Pattern => "<symbol> <above|below> <price>";
        public string Description => $"Remove a price alert for a stock. e.g: /{Command} NVDA above 300";

        private readonly IYahooMarketDataProvider _scraper;
        private readonly ITelegramBotClient _telegramClient;
        private readonly IServiceScopeFactory _scopes;
        private readonly ICurrencySymbolProvider _currency;

        public RemoveAlertCommandHandler(
            IYahooMarketDataProvider scraper,
            ITelegramBotClient telegramClient,
            IServiceScopeFactory scopes,
            ICurrencySymbolProvider currency)
        {
            _scraper = scraper;
            _telegramClient = telegramClient;
            _scopes = scopes;
            _currency = currency;
        }

        public async Task HandleMessage(Message message)
        {
            using var scope = _scopes.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var chatRepo = scope.ServiceProvider.GetRequiredService<IReadRepository<Chat>>();
            var stockRepo = scope.ServiceProvider.GetRequiredService<IRepository<Stock>>();
            var alertRepo = scope.ServiceProvider.GetRequiredService<IRepository<PriceAlert>>();

            var parts = message.Text!
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 4 ||
                !Enum.TryParse<PriceDirection>(
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

            var chat = await chatRepo.FirstOrDefaultAsync(new ChatByTelegramIdSpec(message.Chat.Id));
            var stock = await stockRepo.FirstOrDefaultAsync(new StockBySymbolSpec(symbol));

            if (stock == null)
            {
                await _telegramClient.SendMessage(message.Chat.Id,
                    $"Stock symbol {symbol} not found");
                return;
            }

            var alert = await alertRepo.FirstOrDefaultAsync(
                new AlertByStockAndChatSpec(chat!.Id, stock.Id, direction, price)
            );

            if (alert == null)
            {
                await _telegramClient.SendMessage(message.Chat.Id,
                    $"No alert found for '{symbol}' with direction '{direction}' and price '{price}'.");
                return;
            }

            var otherExists = await alertRepo.AnyAsync(new OtherAlertsByStockIdSpec(alert.StockId, alert.Id));
            if (!otherExists)
            {
                stock.UnWatch();
            }

            alertRepo.Remove(alert);
            await unitOfWork.SaveChangesAsync();

            await _telegramClient.SendMessage(
                message.Chat.Id,
                $"Alert for *{symbol}* with direction *{direction.ToString().ToLower()}* and price *{_currency.GetSymbol(stock.Currency) ?? stock.Currency}{price}* has been removed."
            );
        }
    }
}