using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Commands;

public interface ITelegramCommandHandler
{
    string Command { get; }
    string Description { get; }

    Task<TelegramCommandDispatchResult> Handle(TelegramCommandRequest request, CancellationToken ct);
}
