using System.Text;
using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.Validations;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.CommandHandlers;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;

public class TelegramWebhookRouter : ITelegramWebhookRouter
{
    private readonly IReadOnlyDictionary<string, ITelegramCommandHandler> _handlers;
    private readonly ILogger<ITelegramWebhookRouter> _logger;

    public TelegramWebhookRouter(
        IEnumerable<ITelegramCommandHandler> handlers,
        ILogger<ITelegramWebhookRouter> logger)
    {
        _handlers = handlers.ToDictionary(x => x.Command, StringComparer.OrdinalIgnoreCase);
        _logger = logger;
    }

    public async Task<TelegramUpdateResultDto?> RouteAsync(TelegramUpdateDto update, CancellationToken ct)
    {
        if (update.Command is null)
            return null;

        if (!_handlers.TryGetValue(update.Command, out var handler))
        {
            return CreateResponse(update.ChatId, "Unknown Command");
        }

        try
        {
            return await handler.Handle(update, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to route telegram command {Command}", update.Command);
            return CreateResponse(update.ChatId, ex);
        }
    }

    private static TelegramUpdateResultDto CreateResponse(long chatId, string text)
    {
        return new TelegramUpdateResultDto
        {
            ChatId = chatId,
            Text = text
        };
    }

    private TelegramUpdateResultDto CreateResponse(long chatId, Exception ex)
    {
        switch (ex)
        {
            case ValidationException exc:
            {
                StringBuilder sb = new();
                sb.AppendLine(exc.Message);
                foreach (var err in exc.Errors)
                    sb.AppendLine($"- {err.ErrorMessage}");
            
                return new TelegramUpdateResultDto()
                {
                    ChatId = chatId,
                    Text = sb.ToString(),
                    Method = nameof(ParseMode.Markdown)
                };
            }
            case BaseException baseExc:
                return CreateResponse(chatId, baseExc.Message);
            default:
                return CreateResponse(chatId, "Internal Server Error");
        }
    }
}