using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers
{
    public class HelpCommandHandler : ITelegramCommandHandler
    {
        public string Command => TelegramCommands.Help;
        public string Pattern => string.Empty;
        public string Description => "List all available commands";

        private readonly ITelegramBotClient _bot;
        private readonly IServiceProvider _sp;

        public HelpCommandHandler(
            ITelegramBotClient bot,
            IServiceProvider sp)
        {
            _bot = bot;
            _sp = sp;
        }


        public async Task HandleMessage(Message message)
        {
            var cmds = _sp.GetServices<ITelegramCommandHandler>()
                         .Where(h => h.GetType() != typeof(HelpCommandHandler) &&
                                     h.GetType() != typeof(UnknownCommandHandler));

            var helpText = string.Join("\n",
                cmds.Select((h, i) => $"{i + 1}. *{h.Command}* — {h.Description}"));

            await _bot.SendMessage(
                chatId: message.Chat.Id,
                text: helpText,
                parseMode: ParseMode.Markdown
            );
        }
    }
}
