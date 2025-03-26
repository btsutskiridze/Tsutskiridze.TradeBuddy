using Telegram.Bot.Types;
using Tsutskiridze.TradeBuddy.Helpers;

namespace Tsutskiridze.TradeBuddy.Services.Telegram.MessageHandlers
{
    public class UnknownCommandHandler : ITelegramMessageHandler
    {
        public string Command => TelegramCommands.Unknown;

        public Task HandleMessage(Message message)
        {
            throw new NotImplementedException();
        }
    }
}
