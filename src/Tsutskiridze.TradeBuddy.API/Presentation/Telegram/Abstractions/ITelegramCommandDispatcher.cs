using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Abstractions;

public interface ITelegramCommandDispatcher
{
    Task<TelegramCommandDispatchResult> Dispatch(TelegramCommandRequest request, CancellationToken ct);
}