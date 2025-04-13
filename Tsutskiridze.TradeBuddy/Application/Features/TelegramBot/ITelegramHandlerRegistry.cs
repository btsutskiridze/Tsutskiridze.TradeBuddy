using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers;

namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot
{
    public interface ITelegramHandlerRegistry
    {
        ITelegramMessageHandler GetHandler(string command);
    }
}
