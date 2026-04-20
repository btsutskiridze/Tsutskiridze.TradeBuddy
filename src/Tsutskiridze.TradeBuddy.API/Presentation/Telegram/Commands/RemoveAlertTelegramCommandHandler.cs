using Mediator;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Errors;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.RemoveAlert;
using Tsutskiridze.TradeBuddy.Core.Enums;

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

    public async Task<TelegramCommandDispatchResult> Handle(TelegramCommandRequest request, CancellationToken ct)
    {
        if (request.Args.Count != 3 || !Enum.TryParse<PriceDirection>(
                request.Args[1], true, out var direction) ||
            !decimal.TryParse(request.Args[2], out var price))
        {
            throw new TelegramPresentationException($"Usage: /{Command} NVDA above 300");
        }

        var symbol = request.Args[0];

        var result = await _mediator.Send(new RemoveAlertCommand(request.ChatId, symbol, direction, price), ct);
        
        return TelegramCommandDispatchResult.TextReply(request.ChatId, result.Message);
    }
}