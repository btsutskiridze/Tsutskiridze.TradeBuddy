using System.ComponentModel.DataAnnotations;
using Mediator;
using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Commands;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.CommandHandlers;

public class ActivateChatTelegramCommandHandler : ITelegramCommandHandler
{
    private readonly IMediator _mediator;

    public ActivateChatTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Command => TelegramCommandCatalog.Activate.Command;
    public string Description => TelegramCommandCatalog.Activate.Description;
    public async Task<TelegramUpdateResultDto?> Handle(TelegramUpdateDto update, CancellationToken ct)
    {
        var token = update.Args.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(token))
            throw new ValidationException("Usage: /activate <token>");

        return await _mediator.Send(new ActivateBotCommand(update.ChatId, token), ct);
    }
}
