using Mediator;
using SharedKernel.Validations;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis.Commands.StockQuote;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.CommandHandlers;

public class StockQuoteTelegramCommandHandler : ITelegramCommandHandler
{
    private readonly IMediator _mediator;
    private readonly ITelegramSender _sender;

    public StockQuoteTelegramCommandHandler(IMediator mediator, ITelegramSender sender)
    {
        _mediator = mediator;
        _sender = sender;
    }

    public string Command => TelegramCommandCatalog.StockQuote.Command;
    public string Description => TelegramCommandCatalog.StockQuote.Description;

    public async Task<TelegramUpdateResultDto?> Handle(TelegramUpdateDto update, CancellationToken ct)
    {
        var symbol = update.Args.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(symbol))
            throw new ValidationException("Usage: /quote <symbol>");

        var result = await _mediator.Send(new StockQuoteCommand(update.ChatId, symbol), ct);

        if (!string.IsNullOrWhiteSpace(result.AnalysisMessage))
            await _sender.SendMessage(update.ChatId, result.AnalysisMessage, ct);

        return new TelegramUpdateResultDto
        {
            ChatId = update.ChatId,
            Text = result.SummaryMessage
        };
    }
}
