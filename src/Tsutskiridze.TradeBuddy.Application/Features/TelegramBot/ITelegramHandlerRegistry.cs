using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers;

namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot
{
    public interface ITelegramHandlerRegistry
    {
        ITelegramCommandHandler GetHandler(string command);
        IEnumerable<ITelegramCommandHandler> GetHandlers();
    }
}
