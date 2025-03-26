using Tsutskiridze.TradeBuddy.Helpers;
using Tsutskiridze.TradeBuddy.Services.Telegram.MessageHandlers;

namespace Tsutskiridze.TradeBuddy.Services.Telegram
{
    public interface ITelegramHandlerRegistry
    {
        ITelegramMessageHandler GetHandler(string command);
    }

    public class TelegramHandlerRegistry : ITelegramHandlerRegistry
    {
        private readonly Dictionary<string, ITelegramMessageHandler> _handlers;

        public TelegramHandlerRegistry(IEnumerable<ITelegramMessageHandler> handlers)
        {
            _handlers = handlers.ToDictionary(h => h.Command, h => h);
        }

        public ITelegramMessageHandler GetHandler(string command)
        {
            return _handlers.TryGetValue(command, out var handler) ? handler : _handlers[TelegramCommands.Unknown];
        }
    }

}
