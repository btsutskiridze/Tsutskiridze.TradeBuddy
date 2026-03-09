using Telegram.Bot.Types;
using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Mapping;

public static class TelegramMappingExtensions
{
    public static TelegramUpdateDto ToDto(Update update)
    {
        var text = update.Message?.Text;
        
        if (string.IsNullOrWhiteSpace(text))
            return new TelegramUpdateDto(
                update.Message!.Chat.Id,
                null,
                null,
                []
            );

        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var command = parts[0];
        var args = parts.Skip(1).ToArray();

        return new TelegramUpdateDto(
            update.Message!.Chat.Id,
            text,
            command,
            args
        );
    }
    
}