using Mediator;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Errors;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Commands;

public class StockQuoteTelegramCommandHandler : ITelegramCommandHandler
{
    private readonly IMediator _mediator;

    public StockQuoteTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Command => TelegramCommandCatalog.StockQuote.Command;
    public string Description => TelegramCommandCatalog.StockQuote.Description;

    public async Task<TelegramCommandDispatchResponse> Handle(TelegramCommandDispatchRequest dispatchRequest, CancellationToken ct)
    {
        if (dispatchRequest.Args.Count != 1)
            throw new TelegramPresentationException("Usage: /price <symbol>");
        
        var symbol = dispatchRequest.Args[0];

        if (string.IsNullOrWhiteSpace(symbol))
            throw new TelegramPresentationException("Usage: /price <symbol>");

        var result = await _mediator.Send(new AnalyseStockCommand(dispatchRequest.ChatId, symbol), ct);

        var priorMessages = new List<TelegramOutgoingMessage>();
        if (!string.IsNullOrWhiteSpace(result.AnalysisMessage))
            priorMessages.Add(new TelegramOutgoingMessage(dispatchRequest.ChatId, result.AnalysisMessage));

        return TelegramCommandDispatchResponse.TextReply(
            dispatchRequest.ChatId, 
            result.SummaryMessage, 
            priorMessages: priorMessages
        );
    }
}