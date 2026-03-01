using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Helpers;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Yahoo;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Core.Constants;
using Tsutskiridze.TradeBuddy.Core.Enums;
using Chat = Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Chat;


namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers
{
    public class AlertCommandHandler : ITelegramCommandHandler
    {
        public string Command => TelegramCommands.Alert;
        public string Pattern => "<symbol> <above|below> <price>";
        public string Description => $"Set a price alert for a stock. e.g: /{Command} NVDA above 300";

        private readonly IYahooStockScraper _scraper;
        private readonly ITelegramBotClient _telegramClient;
        private readonly IServiceScopeFactory _scopes;
        private readonly ICurrencySymbolProvider _currency;

        public AlertCommandHandler(
            IYahooStockScraper scraper,
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
            var stockRepository = scope.ServiceProvider.GetRequiredService<IRepository<Stock>>();
            var alertRepository = scope.ServiceProvider.GetRequiredService<IRepository<PriceAlert>>();
            var chatReadRepository = scope.ServiceProvider.GetRequiredService<IReadRepository<Chat>>();

            var parts = message.Text!
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 4 ||
                !Enum.TryParse<PriceAlertDirection>(
                    parts[2], true, out var direction) ||
                !decimal.TryParse(parts[3], out var price))
            {
                await _telegramClient.SendMessage(message.Chat.Id,
                    $"e.g: /{Command} NVDA above(or below) 300");
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

            var chat = await chatReadRepository.FirstOrDefaultAsync(x => x.TelegramChatId == message.Chat.Id);
            var stock = await stockRepository.FirstOrDefaultAsync(x => x.Symbol == symbol);

            if (stock == null)
            {
                stock = new Stock(symbol, quote.Currency, quote.Name);
                await stockRepository.AddAsync(stock);
            }
            
            stock.Watch();
            var alert = new PriceAlert(Guid.NewGuid(), chat!.Id, stock.Id, price, direction);
            await alertRepository.AddAsync(alert);
            await unitOfWork.SaveChangesAsync();

            await _telegramClient.SendMessage(message.Chat.Id,
                $"✅ Price alert set for {symbol} {direction.ToString().ToLower()} {_currency.GetSymbol(quote.Currency) ?? quote.Currency}{price}"
            );
        }
    }
}