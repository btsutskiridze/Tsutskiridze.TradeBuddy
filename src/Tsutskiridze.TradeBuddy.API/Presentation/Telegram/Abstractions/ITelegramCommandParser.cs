using Tsutskiridze.TradeBuddy.API.Contracts.Telegram;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Abstractions;

public interface ITelegramCommandParser
{
    TelegramCommandDispatchRequest? Parse(TelegramWebhookRequest request);
}