using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.API.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Telegram.Commands;

public class HelpTelegramCommandHandler : ITelegramCommandHandler
{
    public string Command => TelegramCommandCatalog.Help.Command;
    public string Description => TelegramCommandCatalog.Help.Description;
    public Task<TelegramCommandDispatchResponse> Handle(TelegramCommandDispatchRequest dispatchRequest, CancellationToken ct)
    {
        var helpText = string.Join("\n\n",
            TelegramCommandCatalog.All.Select((h, i) =>
                $"{i + 1}. *{h.Command}* - {h.Description}\n{FormatUsage(h.Usage)}"));

        return Task.FromResult(
            TelegramCommandDispatchResponse.TextReply(dispatchRequest.ChatId, helpText, ParseMode.Markdown)
        );
    }

    private static string FormatUsage(string usage)
    {
        return string.Join("\n", usage.Split('\n').Select(x => $"`{x}`"));
    }
}
