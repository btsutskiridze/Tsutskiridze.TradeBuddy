using System.Text;
using SharedKernel;
using SharedKernel.Validations;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Abstractions;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Errors;

public class TelegramErrorResponseFactory : ITelegramErrorResponseFactory
{
    public TelegramCommandDispatchResult Create(long chatId, Exception exception)
    {
        return exception switch
        {
            TelegramPresentationException ex =>
                TelegramCommandDispatchResult.TextReply(chatId, ex.Message, ParseMode.Markdown),

            ValidationException ex =>
                TelegramCommandDispatchResult.TextReply(chatId, BuildValidationMessage(ex), ParseMode.Markdown),

            BaseException ex =>
                TelegramCommandDispatchResult.TextReply(chatId, ex.Message),

            _ =>
                TelegramCommandDispatchResult.TextReply(chatId, "Internal server error")
        };
    }

    private static string BuildValidationMessage(ValidationException ex)
    {
        if (ex.Errors.Count == 0)
            return ex.Message;

        StringBuilder sb = new();
        sb.AppendLine(ex.Message);
        foreach (var err in ex.Errors)
            sb.AppendLine($"- {err.ErrorMessage}");

        return sb.ToString();
    }
}