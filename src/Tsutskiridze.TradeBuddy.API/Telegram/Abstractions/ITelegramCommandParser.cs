using Tsutskiridze.TradeBuddy.API.Contracts.Telegram;
using Tsutskiridze.TradeBuddy.API.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Telegram.Abstractions;

public interface ITelegramCommandParser
{
    TelegramCommandDispatchRequest? Parse(TelegramWebhookRequest request);
}