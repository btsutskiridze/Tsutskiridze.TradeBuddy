using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Database;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Helpers;
using Tsutskiridze.TradeBuddy.Core.Constants;
using Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram;

namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers
{
    public class MyAlertsCommandHandler : ITelegramCommandHandler
    {
        public string Command => TelegramCommands.MyAlerts;
        public string Description => "List all your alerts";

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
            var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            await _bot.SendChatAction(message.Chat.Id, ChatAction.Typing);

            var priceAlerts = db.Set<PriceAlert>()
                .Where(x => x.Chat.TelegramChatID == message.Chat.Id)
                .Include(x => x.Chat)
                .Include(x => x.Stock)
                .ToList();

            if (priceAlerts.Count == 0)
            {
                await _bot.SendMessage(message.Chat.Id, "You have no alerts set.");
                return;
            }

            var alertList = priceAlerts
                .Select(x => $"{x.Stock.Symbol} {x.Direction} {_currency.GetSymbol(x.Stock.Currency)}{x.Price}")
                .ToList();

            await _bot.SendMessage(
                chatId: message.Chat.Id,
                text: $"Your alerts:\n" +
                      $"{string.Join("\n", alertList)}",
                parseMode: ParseMode.Markdown
            );
        }
    }
}
