using Mediator;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Errors;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock.Models;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Commands;

public class StockQuoteTelegramCommandHandler : ITelegramCommandHandler
{
    private readonly IMediator _mediator;

    public StockQuoteTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Command => TelegramCommandCatalog.AnalyseStock.Command;
    public string Description => TelegramCommandCatalog.AnalyseStock.Description;

    public async Task<TelegramCommandDispatchResponse> Handle(TelegramCommandDispatchRequest dispatchRequest, CancellationToken ct)
    {
        if (dispatchRequest.Args.Count != 1)
            throw new TelegramPresentationException("Usage: /price <symbol>");
        
        var symbol = dispatchRequest.Args[0];

        if (string.IsNullOrWhiteSpace(symbol))
            throw new TelegramPresentationException("Usage: /price <symbol>");

        var result = await _mediator.Send(new AnalyseStockCommand(dispatchRequest.ChatId, symbol), ct);

        return TelegramCommandDispatchResponse.TextReply(
            dispatchRequest.ChatId, 
            CreateAnalysisMessage(result)
        );
    }
    
    private static string CreateAnalysisMessage(AnalyseStockResult analysis)
    {
        return $"🚨 Stock Alert: {analysis.Symbol} 🚨\n" +
               $"- 📈 Current Price: {analysis.price}\n" +
               $"- 📊 50-day Avg: {analysis.bench.avg50} | Year High: {analysis.bench.yearHigh}\n" +
               $"- 🔔 Trading Volume: {analysis.volAnalysis}\n" +
               $"- 📰 Overall News: {analysis.newsOverall.conf} Positive\n" +
               $"- 🤖 AI Analysis: {analysis.ai.rec} ({analysis.ai.conf} Confidence)\n" +
               $"- ⏱️ Analysis Duration: {analysis.ExecutionTime:0.00}s";
    }
}