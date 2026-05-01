using Telegram.Bot.Types.Enums;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram;

public sealed record TelegramOutgoingMessage(long ChatId, string Text, ParseMode ParseMode = ParseMode.Markdown);