using Tsutskiridze.TradeBuddy.API.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Telegram.Abstractions;

public interface ITelegramCommandDispatcher
{
    Task<TelegramCommandDispatchResponse> Dispatch(TelegramCommandDispatchRequest dispatchRequest, CancellationToken ct);
}