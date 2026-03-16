using Mediator;
using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.CommandHandlers;

public class CreateAlertTelegramCommandHandler : ITelegramCommandHandler
{
    private readonly IMediator _mediator;

    public CreateAlertTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public string Command => "/alert";

    public async Task Handle(TelegramUpdateDto update, CancellationToken ct)
    {
    }
}