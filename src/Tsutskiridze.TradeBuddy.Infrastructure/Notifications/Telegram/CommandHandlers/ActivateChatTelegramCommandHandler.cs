using System.ComponentModel.DataAnnotations;
using Mediator;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Commands;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Contracts;

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
    public async Task<TelegramMessageResponse?> Handle(TelegramMessageRequest update, CancellationToken ct)
    {
        var token = update.Args.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(token))
            throw new ValidationException("Usage: /activate <token>");

        var result = await _mediator.Send(new ActivateChatCommand(update.ChatId, token), ct);

        return new TelegramMessageResponse
        {
            ChatId = update.ChatId,
            Text = result.Message
        };
    }
}
