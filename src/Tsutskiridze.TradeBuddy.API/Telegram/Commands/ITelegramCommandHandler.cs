using Tsutskiridze.TradeBuddy.API.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Telegram.Commands;

public interface ITelegramCommandHandler
{
    string Command { get; }
    string Description { get; }

    Task<TelegramCommandDispatchResponse> Handle(TelegramCommandDispatchRequest dispatchRequest, CancellationToken ct);
}
