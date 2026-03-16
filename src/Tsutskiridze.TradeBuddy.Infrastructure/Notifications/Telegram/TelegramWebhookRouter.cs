using System.ComponentModel.DataAnnotations;
using Mediator;
using Microsoft.Extensions.Logging;
using SharedKernel;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.CommandHandlers;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;

public class TelegramWebhookRouter : ITelegramWebhookRouter
{
    private readonly IMediator _mediator;
    private readonly IReadOnlyDictionary<string, ITelegramCommandHandler> _handlers;
    private readonly ILogger<ITelegramWebhookRouter> _logger;

    public TelegramWebhookRouter(
        IMediator mediator,
        IEnumerable<ITelegramCommandHandler> handlers,
        ILogger<ITelegramWebhookRouter> logger)
    {
        _mediator = mediator;
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
            await handler.Handle(update, ct);
            return null;
        }
        catch (Exception ex) when (ex is BaseException or ValidationException)
        {
            return CreateResponse(update.ChatId, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Telegram command {Command} for chat {ChatId}",
                update.Command, update.ChatId);

            return CreateResponse(update.ChatId, "Internal server error.");
        }
    }
    
    private TelegramUpdateResultDto CreateResponse(long chatId, string message)
    {
        return new TelegramUpdateResultDto()
        {
            ChatId = chatId,
            Text = message
        };
    }
}