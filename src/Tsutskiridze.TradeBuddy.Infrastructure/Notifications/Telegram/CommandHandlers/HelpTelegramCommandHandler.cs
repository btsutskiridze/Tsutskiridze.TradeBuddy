using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.CommandHandlers;

public class HelpTelegramCommandHandler : ITelegramCommandHandler
{
    public string Command => TelegramCommandCatalog.Help.Command;
    public string Description => TelegramCommandCatalog.Help.Description;
    public Task<TelegramMessageResponse?> Handle(TelegramMessageRequest update, CancellationToken ct)
    {
        var helpText = string.Join("\n",
            TelegramCommandCatalog.All.Select((h, i) => $"{i + 1}. *{h.Command}* � {h.Description}"));

        return Task.FromResult<TelegramMessageResponse?>(new TelegramMessageResponse()
        {
            ChatId = update.ChatId,
            Text = helpText,
            ParseMode = nameof(ParseMode.Markdown)
        });
    }
}
