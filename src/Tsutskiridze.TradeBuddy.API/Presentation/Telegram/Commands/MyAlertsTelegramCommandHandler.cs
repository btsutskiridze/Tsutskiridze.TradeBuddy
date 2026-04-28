using Mediator;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Queries.MyAlerts;

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
    public async Task<TelegramCommandDispatchResponse> Handle(TelegramCommandDispatchRequest dispatchRequest, CancellationToken ct)
    {
        var result = await _mediator.Send(new MyAlertsCommand(dispatchRequest.ChatId), ct);
        
        return TelegramCommandDispatchResponse.TextReply(dispatchRequest.ChatId, result.Message, ParseMode.Markdown);
    }
}
