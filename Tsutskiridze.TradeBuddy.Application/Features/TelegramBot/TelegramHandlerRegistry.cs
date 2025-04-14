using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers;
using Tsutskiridze.TradeBuddy.Core.Constants;

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
            return _handlers.TryGetValue(command, out var handler) ? handler : _handlers[TelegramCommands.Unknown];
        }
    }

}
