using Mediator;
using System.Text;
using SharedKernel.Localization;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Queries.GetMyAlerts;

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
        var result = await _mediator.Send(new MyAlertsQuery(dispatchRequest.ChatId), ct);
        
        return TelegramCommandDispatchResponse.TextReply(dispatchRequest.ChatId, CreateMessage(result), ParseMode.Markdown);
    }

    private string CreateMessage(MyAlertsResult result)
    {
        var lines = result.Alerts
            .Select((alert, index) =>
            {
                var currencySymbol = CurrencySymbolLookup.GetSymbol(alert.CurrencyCode) ?? alert.CurrencyCode;
                return $"{index + 1}. *{alert.Symbol}* {alert.Direction} {currencySymbol}{alert.Price}";
            });

        return new StringBuilder()
            .AppendLine("🔔 *Your Active Alerts* 🔔")
            .AppendLine()
            .AppendJoin("\n", lines)
            .ToString();
    }
}
