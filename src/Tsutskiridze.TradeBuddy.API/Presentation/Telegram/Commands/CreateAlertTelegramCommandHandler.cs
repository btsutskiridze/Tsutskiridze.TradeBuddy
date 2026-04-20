using Mediator;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Errors;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.CreateAlert;
using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Commands;

public class CreateAlertTelegramCommandHandler : ITelegramCommandHandler
{
    private readonly IMediator _mediator;

    public CreateAlertTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Command => TelegramCommandCatalog.Alert.Command;
    public string Description => TelegramCommandCatalog.Alert.Description;
    public async Task<TelegramCommandDispatchResult> Handle(TelegramCommandRequest request, CancellationToken ct)
    {
        if (request.Args.Count != 3)
            throw new TelegramPresentationException("Usage: `/set NVDA above 300`");

        var symbol = request.Args[0];

        if (!Enum.TryParse<PriceDirection>(request.Args[1], true, out var direction))
            throw new TelegramPresentationException("Direction must be `above` or `below`.");

        if (!decimal.TryParse(request.Args[2], out var price))
            throw new TelegramPresentationException("Price must be a valid decimal number.");
        
        var result = await _mediator.Send(new CreateAlertCommand(request.ChatId, symbol, direction, price), ct);

        return TelegramCommandDispatchResult.TextReply(request.ChatId, result.Message);
    }
}
