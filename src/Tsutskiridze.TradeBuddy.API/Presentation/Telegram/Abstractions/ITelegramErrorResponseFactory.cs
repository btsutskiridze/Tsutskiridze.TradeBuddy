using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Abstractions;

public interface ITelegramErrorResponseFactory
{
    TelegramCommandDispatchResult Create(long chatId, Exception exception);
}