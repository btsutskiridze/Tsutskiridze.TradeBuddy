using System.ComponentModel.DataAnnotations;
using Mediator;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.RemoveAlert;
using Tsutskiridze.TradeBuddy.Core.Enums;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.CommandHandlers;

public class RemoveAlertTelegramCommandHandler : ITelegramCommandHandler
{
    private readonly IMediator _mediator;

    public RemoveAlertTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Command => TelegramCommandCatalog.RemoveAlert.Command;
    public string Description => TelegramCommandCatalog.RemoveAlert.Description;

    public async Task<TelegramUpdateResultDto?> Handle(TelegramUpdateDto update, CancellationToken ct)
    {
        if (update.Args.Length != 3 || !Enum.TryParse<PriceDirection>(
                update.Args[1], true, out var direction) ||
            !decimal.TryParse(update.Args[2], out var price))
        {
            throw new ValidationException($"Usage: /{Command} NVDA above 300");
        }

        var symbol = update.Args[0];

        var result = await _mediator.Send(new RemoveAlertCommand(update.ChatId, symbol, direction, price), ct);
        
        return new TelegramUpdateResultDto
        {
            ChatId = update.ChatId,
            Text = result.Message
        };
    }
}