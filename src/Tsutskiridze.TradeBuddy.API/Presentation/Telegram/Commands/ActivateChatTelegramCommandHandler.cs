using Mediator;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Errors;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Commands.ActivateChat;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Commands;

public class ActivateChatTelegramCommandHandler : ITelegramCommandHandler
{
    private readonly IMediator _mediator;

    public ActivateChatTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Command => TelegramCommandCatalog.Activate.Command;
    public string Description => TelegramCommandCatalog.Activate.Description;
    public async Task<TelegramCommandDispatchResponse> Handle(TelegramCommandDispatchRequest dispatchRequest, CancellationToken ct)
    {
        if(dispatchRequest.Args.Count != 1)
            throw new TelegramPresentationException("Usage: /activate <token>");
        
        var token = dispatchRequest.Args[0];

        if (string.IsNullOrWhiteSpace(token))
            throw new TelegramPresentationException("Usage: /activate <token>");

        var result = await _mediator.Send(new ActivateChatCommand(dispatchRequest.ChatId, token), ct);

        return TelegramCommandDispatchResponse.TextReply(dispatchRequest.ChatId, result.Message);
    }
}
