using Telegram.Bot.Types;

namespace Tsutskiridze.TradeBuddy.Services.Telegram.MessageHandlers
{
    public interface ITelegramMessageHandler
    {
        string Command { get; }
        Task HandleMessage(Message message);
    }
}
