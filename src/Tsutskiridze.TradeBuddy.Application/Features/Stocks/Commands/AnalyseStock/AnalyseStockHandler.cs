using Mediator;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Application.Abstractions.RateLimiting;
using Tsutskiridze.TradeBuddy.Application.DTOs.StockAnalysis;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock.Services;
using Tsutskiridze.TradeBuddy.Application.RateLimiting;

namespace Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock;

public sealed record AnalyseStockCommand(long ChatId, string Symbol) : ICommand<AnalyseStockResult>, IRateLimitedMessage
{
    public string Id => ChatId.ToString();
    public string Policy => RateLimitPolicy.AiAnalysisRequest;
}

public sealed record AnalyseStockResult(string Message);

public class AnalyseStockHandler : ICommandHandler<AnalyseStockCommand, AnalyseStockResult>
{
    private readonly ILogger<AnalyseStockHandler> _logger;
    private readonly StockAnalysisGenerator _stockAnalysisGenerator;
    private readonly IMarketDataProvider _stockScraper;

    public AnalyseStockHandler(
        ILogger<AnalyseStockHandler> logger,
        StockAnalysisGenerator stockAnalysisGenerator,
        IMarketDataProvider stockScraper)
    {
        _logger = logger;
        _stockAnalysisGenerator = stockAnalysisGenerator;
        _stockScraper = stockScraper;
    }

    public async ValueTask<AnalyseStockResult> Handle(AnalyseStockCommand command, CancellationToken ct)
    {
        var symbol = command.Symbol.Trim().ToUpperInvariant();
        _logger.LogDebug("Received stock command for symbol {StockSymbol}", symbol);

        if (!await _stockScraper.StockSymbolExists(symbol))
        {
            return new AnalyseStockResult("Invalid stock symbol. Please provide a valid stock symbol.");
        }

        var analysis = await _stockAnalysisGenerator.GenerateAsync(symbol);
        var analysisMessage = CreateAnalysisMessage(analysis);

        return new AnalyseStockResult(analysisMessage);
    }

    private static string CreateAnalysisMessage(StockAnalysisReportDto analysis)
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