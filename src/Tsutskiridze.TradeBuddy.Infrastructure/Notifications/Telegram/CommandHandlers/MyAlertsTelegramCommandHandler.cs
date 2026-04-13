using Mediator;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.MyAlerts;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.CommandHandlers;

public class MyAlertsTelegramCommandHandler : ITelegramCommandHandler
{
    private readonly IMediator _mediator;
    
    public MyAlertsTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public string Command => TelegramCommandCatalog.MyAlerts.Command;
    public string Description => TelegramCommandCatalog.MyAlerts.Description;
    public async Task<TelegramUpdateResultDto?> Handle(TelegramUpdateDto update, CancellationToken ct)
    {
        var result = await _mediator.Send(new MyAlertsCommand(update.ChatId), ct);

        return new TelegramUpdateResultDto
        {
            ChatId = update.ChatId,
            Text = result.Message,
            ParseMode = nameof(ParseMode.Markdown)
        };
    }
}
