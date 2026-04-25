using Mediator;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Errors;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.RemoveAlert;
using Tsutskiridze.TradeBuddy.Domain.Enums;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Commands;

public class RemoveAlertTelegramCommandHandler : ITelegramCommandHandler
{
    private readonly IMediator _mediator;

    public RemoveAlertTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Command => TelegramCommandCatalog.RemoveAlert.Command;
    public string Description => TelegramCommandCatalog.RemoveAlert.Description;

    public async Task<TelegramCommandDispatchResponse> Handle(TelegramCommandDispatchRequest dispatchRequest, CancellationToken ct)
    {
        if (dispatchRequest.Args.Count != 3 || !Enum.TryParse<PriceDirection>(
                dispatchRequest.Args[1], true, out var direction) ||
            !decimal.TryParse(dispatchRequest.Args[2], out var price))
        {
            throw new TelegramPresentationException($"Usage: /{Command} NVDA above 300");
        }

        var symbol = dispatchRequest.Args[0];

        var result = await _mediator.Send(new RemoveAlertCommand(dispatchRequest.ChatId, symbol, direction, price), ct);
        
        return TelegramCommandDispatchResponse.TextReply(dispatchRequest.ChatId, result.Message);
    }
}