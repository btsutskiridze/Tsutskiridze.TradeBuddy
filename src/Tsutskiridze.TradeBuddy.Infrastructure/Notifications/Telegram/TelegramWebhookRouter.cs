using System.Text;
using Microsoft.Extensions.Logging;
using SharedKernel;
using SharedKernel.Validations;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.CommandHandlers;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;

public class TelegramWebhookRouter : ITelegramWebhookRouter
{
    private readonly IReadOnlyDictionary<string, ITelegramCommandHandler> _handlers;

    private readonly ITelegramSender _sender;
    private readonly ILogger<ITelegramWebhookRouter> _logger;
    

    public TelegramWebhookRouter(
        ITelegramSender sender,
        IEnumerable<ITelegramCommandHandler> handlers,
        ILogger<ITelegramWebhookRouter> logger)
    {
        _sender = sender;
        _logger = logger;
        _handlers = handlers.ToDictionary(x => x.Command, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<TelegramMessageResponse?> RouteAsync(TelegramMessageRequest update, CancellationToken ct)
    {
        await _sender.SendTypingAction(update.ChatId, ct);
        
        if (update.Command is null)
            return null;

        if (!_handlers.TryGetValue(update.Command, out var handler))
        {
            return CreateResponse(update.ChatId, "Unknown Command");
        }

        try
        {
            await _sender.SendTypingAction(update.ChatId, ct);
            return await handler.Handle(update, ct);
        }
        catch (Exception ex)
        {
            await _sender.SendTypingAction(update.ChatId, ct);
            _logger.LogError(ex, "Failed to route telegram command {Command}", update.Command);
            return CreateResponse(update.ChatId, ex);
        }
    }

    private static TelegramMessageResponse CreateResponse(long chatId, string text)
    {
        return new TelegramMessageResponse
        {
            ChatId = chatId,
            Text = text
        };
    }

    private static TelegramMessageResponse CreateResponse(long chatId, Exception ex)
    {
        switch (ex)
        {
            case ValidationException exc:
            {
                StringBuilder sb = new();
                sb.AppendLine(exc.Message);
                foreach (var err in exc.Errors)
                    sb.AppendLine($"- {err.ErrorMessage}");
            
                return new TelegramMessageResponse()
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
