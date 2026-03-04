using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using SharedKernel;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Helpers;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Core.Constants;
using Chat = Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Chat;

namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers
{
    public class MyAlertsCommandHandler : ITelegramCommandHandler
    {
        public string Command => TelegramCommands.MyAlerts;
        public string Pattern => string.Empty;
        public string Description => $"List all your alerts e.g: /{Command}";

        private readonly ITelegramBotClient _bot;
        private readonly IServiceScopeFactory _scopes;
        private readonly ICurrencySymbolProvider _currency;

        public MyAlertsCommandHandler(
            ITelegramBotClient bot,
            IServiceScopeFactory scopes,
            ICurrencySymbolProvider currency)
        {
            _bot = bot;
            _scopes = scopes;
            _currency = currency;
        }


        public async Task HandleMessage(Message message)
        {
            using var scope = _scopes.CreateScope();
            var chatRepo = scope.ServiceProvider.GetRequiredService<IReadRepository<Chat>>();
            var stockRepo = scope.ServiceProvider.GetRequiredService<IReadRepository<Stock>>();

            await _bot.SendChatAction(message.Chat.Id, ChatAction.Typing);

            var chat = await chatRepo.FirstOrDefaultAsync(
                new ChatByTelegramIdIncludePriceAlertsSpec(message.Chat.Id)
            );

            var priceAlerts = chat!.PriceAlerts;
            if (priceAlerts.Count == 0)
            {
                await _bot.SendMessage(message.Chat.Id, "You have no alerts set.");
                return;
            }

            var stockIds = priceAlerts.Select(x => x.StockId).ToList();
            var stocks = (await stockRepo.ListAsync(new StocksByIdsSpec(stockIds)))
                .ToImmutableDictionary(x => x.Id);
            
            var alertList = priceAlerts
                .Select(x =>
                {
                    var stock = stocks[x.StockId];
                    return $"*{stock.Symbol}* {x.Direction} {_currency.GetSymbol(stock.Currency)}{x.Price}";
                })
                .ToList();

            var text = new StringBuilder()
                .AppendLine("🔔 *Your Active Alerts* 🔔")
                .AppendLine()
                .AppendJoin("\n", alertList
                    .Select((a, i) => $"{i + 1}. {a}"))
                .ToString();

            await _bot.SendMessage(
                chatId: message.Chat.Id,
                parseMode: ParseMode.Markdown,
                text: text
            );
        }
    }
}