using Tsutskiridze.TradeBuddy.API.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Telegram.Abstractions;

public interface ITelegramErrorResponseFactory
{
    TelegramCommandDispatchResponse Create(long chatId, Exception exception);
}