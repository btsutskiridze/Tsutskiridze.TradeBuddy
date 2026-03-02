using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers;

namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot
{
    public class TelegramHandlerRegistry : ITelegramHandlerRegistry
    {
        private readonly Dictionary<string, ITelegramCommandHandler> _handlers;

        public TelegramHandlerRegistry(IEnumerable<ITelegramCommandHandler> handlers)
        {
            _handlers = handlers.ToDictionary(h => h.Command, h => h);
        }

        public ITelegramCommandHandler GetHandler(string command)
        {
            return _handlers.TryGetValue(command, out var handler) ? handler : _handlers["unknown_command"];
        }

        public IEnumerable<ITelegramCommandHandler> GetHandlers()
        {
            return _handlers.Values;
        }
    }

}
