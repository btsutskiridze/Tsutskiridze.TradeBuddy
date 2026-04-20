using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Abstractions;

public interface ITelegramCommandDispatcher
{
    Task<TelegramCommandDispatchResponse> Dispatch(TelegramCommandDispatchRequest dispatchRequest, CancellationToken ct);
}