using Mediator;
using SharedKernel.Validations;
using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.CommandHandlers;

public class StockQuoteTelegramCommandHandler : ITelegramCommandHandler
{
    private readonly IMediator _mediator;

    public StockQuoteTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Command => TelegramCommandCatalog.StockQuote.Command;
    public string Description => TelegramCommandCatalog.StockQuote.Description;

    public async Task<TelegramUpdateResultDto?> Handle(TelegramUpdateDto update, CancellationToken ct)
    {
        var symbol = update.Args.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(symbol))
            throw new ValidationException("Usage: /activate <token>");

        return await _mediator.Send(new StockQuoteCommand(update.ChatId, symbol), ct);
    }
}