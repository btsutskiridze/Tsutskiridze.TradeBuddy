using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Abstractions;

public interface ITelegramErrorResponseFactory
{
    TelegramCommandDispatchResponse Create(long chatId, Exception exception);
}