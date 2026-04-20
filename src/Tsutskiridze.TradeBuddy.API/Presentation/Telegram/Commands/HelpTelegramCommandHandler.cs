using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Commands;

public class HelpTelegramCommandHandler : ITelegramCommandHandler
{
    public string Command => TelegramCommandCatalog.Help.Command;
    public string Description => TelegramCommandCatalog.Help.Description;
    public Task<TelegramCommandDispatchResult> Handle(TelegramCommandRequest request, CancellationToken ct)
    {
        var helpText = string.Join("\n",
            TelegramCommandCatalog.All.Select((h, i) => $"{i + 1}. *{h.Command}* - {h.Description}"));

        return Task.FromResult(
            TelegramCommandDispatchResult.TextReply(request.ChatId, helpText, ParseMode.Markdown)
        );
    }
}
