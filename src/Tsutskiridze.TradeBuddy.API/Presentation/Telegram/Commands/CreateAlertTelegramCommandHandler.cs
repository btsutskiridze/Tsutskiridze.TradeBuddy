using Mediator;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Errors;
using Tsutskiridze.TradeBuddy.Application.Common.Localization;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.CreateAlert;
using Tsutskiridze.TradeBuddy.Domain.Enums;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Commands;

public class CreateAlertTelegramCommandHandler : ITelegramCommandHandler
{
    private readonly IMediator _mediator;
    private readonly ICurrencySymbolProvider _currencySymbolProvider;

    public CreateAlertTelegramCommandHandler(IMediator mediator, ICurrencySymbolProvider currencySymbolProvider)
    {
        _mediator = mediator;
        _currencySymbolProvider = currencySymbolProvider;
    }

    public string Command => TelegramCommandCatalog.Alert.Command;
    public string Description => TelegramCommandCatalog.Alert.Description;
    public async Task<TelegramCommandDispatchResponse> Handle(TelegramCommandDispatchRequest dispatchRequest, CancellationToken ct)
    {
        if (dispatchRequest.Args.Count != 3)
            throw new TelegramPresentationException("Usage: `/set NVDA above 300`");

        var symbol = dispatchRequest.Args[0];

        if (!Enum.TryParse<PriceDirection>(dispatchRequest.Args[1], true, out var direction))
            throw new TelegramPresentationException("Direction must be `above` or `below`.");

        if (!decimal.TryParse(dispatchRequest.Args[2], out var price))
            throw new TelegramPresentationException("Price must be a valid decimal number.");
        
        var result = await _mediator.Send(new CreateAlertCommand(dispatchRequest.ChatId, symbol, direction, price), ct);

        return TelegramCommandDispatchResponse.TextReply(dispatchRequest.ChatId, CreateMessage(result));
    }

    private string CreateMessage(CreateAlertResult result)
    {
        var currencySymbol = _currencySymbolProvider.GetSymbol(result.CurrencyCode) ?? result.CurrencyCode;

        return $"✅ Price alert set for {result.Symbol} {result.Direction} {currencySymbol}{result.Price}";
    }
}
