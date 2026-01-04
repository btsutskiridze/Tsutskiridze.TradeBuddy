using Telegram.Bot.Types;

namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers
{
    public interface ITelegramCommandHandler
    {
        string Command { get; }
        string Pattern { get; }
        string Description { get; }
        Task HandleMessage(Message message);
    }
}
