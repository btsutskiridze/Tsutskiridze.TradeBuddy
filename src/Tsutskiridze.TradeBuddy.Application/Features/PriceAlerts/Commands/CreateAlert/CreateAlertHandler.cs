using System.ComponentModel.DataAnnotations;
using Mediator;
using SharedKernel;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.Exceptions;
using Tsutskiridze.TradeBuddy.Application.Notifications.PriceAlerts;
using Tsutskiridze.TradeBuddy.Core.Enums;
using Tsutskiridze.TradeBuddy.Core.Services;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.CreateAlert;

public record CreateAlertCommand(long ChatId, string Symbol, PriceDirection Direction, decimal Price) : ICommand;

public class CreateAlertHandler : ICommandHandler<CreateAlertCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly IAlertDomainService _service;
    private readonly ITelegramSender _sender;
    private readonly IYahooMarketDataProvider _yahoo;
    private readonly IMediator _mediator;

    public CreateAlertHandler(IUnitOfWork uow, IAlertDomainService service, ITelegramSender sender,
        IYahooMarketDataProvider yahoo, IMediator mediator)
    {
        _uow = uow;
        _service = service;
        _sender = sender;
        _yahoo = yahoo;
        _mediator = mediator;
    }

    public async ValueTask<Unit> Handle(CreateAlertCommand command, CancellationToken cancellationToken)
    {
        await _sender.SendTypingAction(command.ChatId, cancellationToken);

        if (!await _yahoo.StockSymbolExits(command.Symbol))
        {
            throw new ValidationException("Stock Symbol not found");
        }

        var stockQuote = await _yahoo.GetStockQuote(command.Symbol);

        if (stockQuote is null)
        {
            throw new ApplicationLayerException("Failed to fetch quote for the stock");
        }

        await _service.CreateAlertAsync(
            command.ChatId,
            command.Symbol,
            stockQuote.Currency,
            stockQuote.Name,
            command.Price,
            command.Direction,
            cancellationToken
        );

        await _uow.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(
            new PriceAlertActivatedNotification(command.ChatId, command.Symbol, command.Direction, stockQuote.Currency, command.Price), 
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}