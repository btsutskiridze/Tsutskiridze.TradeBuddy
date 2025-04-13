using Telegram.Bot.Types;

namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers
{
    public interface ITelegramMessageHandler
    {
        string Command { get; }
        Task HandleMessage(Message message);
    }
}
