using Mediator;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Application.Abstractions.RateLimiting;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Services;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock.Models;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock.Services;
using Tsutskiridze.TradeBuddy.Application.RateLimiting;

namespace Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock;

public sealed record AnalyseStockCommand(long ChatId, string Symbol) : ICommand<AnalyseStockResult>, IRateLimitedMessage
{
    public string Id => ChatId.ToString();
    public string Policy => RateLimitPolicy.AiAnalysisRequest;
}

public class AnalyseStockHandler : ICommandHandler<AnalyseStockCommand, AnalyseStockResult>
{
    private readonly IActiveChatProvider _activeChatProvider;
    private readonly ILogger<AnalyseStockHandler> _logger;
    private readonly StockAnalysisGenerator _stockAnalysisGenerator;
    private readonly IMarketDataProvider _stockScraper;

    public AnalyseStockHandler(
        ILogger<AnalyseStockHandler> logger,
        StockAnalysisGenerator stockAnalysisGenerator,
        IMarketDataProvider stockScraper, IActiveChatProvider activeChatProvider)
    {
        _logger = logger;
        _stockAnalysisGenerator = stockAnalysisGenerator;
        _stockScraper = stockScraper;
        _activeChatProvider = activeChatProvider;
    }

    public async ValueTask<AnalyseStockResult> Handle(AnalyseStockCommand command, CancellationToken ct)
    {
        await _activeChatProvider.ThrowIfNotFound(command.ChatId, ct);

        var symbol = command.Symbol.Trim().ToUpperInvariant();
        _logger.LogDebug("Received stock command for symbol {StockSymbol}", symbol);

        if (!await _stockScraper.StockSymbolExists(symbol))
        {
            throw new ApplicationLayerException("Invalid stock symbol. Please provide a valid stock symbol.");
        }

        var analysis = await _stockAnalysisGenerator.GenerateAsync(symbol);

        return analysis;
    }
}