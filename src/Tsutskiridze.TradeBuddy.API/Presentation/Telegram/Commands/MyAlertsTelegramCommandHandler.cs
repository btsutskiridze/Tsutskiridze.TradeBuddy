using Mediator;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.MyAlerts;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Commands;

public class MyAlertsTelegramCommandHandler : ITelegramCommandHandler
{
    private readonly IMediator _mediator;
    
    public MyAlertsTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public string Command => TelegramCommandCatalog.MyAlerts.Command;
    public string Description => TelegramCommandCatalog.MyAlerts.Description;
    public async Task<TelegramCommandDispatchResult> Handle(TelegramCommandRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new MyAlertsCommand(request.ChatId), ct);
        
        return TelegramCommandDispatchResult.TextReply(request.ChatId, result.Message, ParseMode.Markdown);
    }
}
