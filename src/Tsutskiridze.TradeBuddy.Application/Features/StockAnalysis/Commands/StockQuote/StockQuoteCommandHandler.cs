using Mediator;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;
using Tsutskiridze.TradeBuddy.Application.DTOs.StockAnalysis;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis.Services;

namespace Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis.Commands.StockQuote;

public sealed record StockQuoteCommand(long ChatId, string Symbol) : ICommand<StockQuoteResult>;

public sealed record StockQuoteResult(string SummaryMessage, string? AnalysisMessage);

public class StockQuoteCommandHandler : ICommandHandler<StockQuoteCommand, StockQuoteResult>
{
    private readonly ILogger<StockQuoteCommandHandler> _logger;
    private readonly StockAnalysisGenerator _stockAnalysisGenerator;
    private readonly IMarketDataProvider _stockScraper;

    
    /*
     *todo:
     * Move quotas/rate limits to
     * Redis, database, or a dedicated policy service.
     */
    private static int _analysisCount = 0;
    private static DateTime _lastReset = DateTime.UtcNow.Date;
    private static DateTime _lastExecution = DateTime.MinValue;
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private const int MaxAnalysisCount = 100;
    private const int DelayBetweenAnalysesSeconds = 5;

    public StockQuoteCommandHandler(
        ILogger<StockQuoteCommandHandler> logger,
        StockAnalysisGenerator stockAnalysisGenerator,
        IMarketDataProvider stockScraper)
    {
        _logger = logger;
        _stockAnalysisGenerator = stockAnalysisGenerator;
        _stockScraper = stockScraper;
    }

    public async ValueTask<StockQuoteResult> Handle(StockQuoteCommand command, CancellationToken ct)
    {
        var symbol = command.Symbol.Trim();

        int analysesLeftAfterReservation;
        bool reservationMade;
        await Gate.WaitAsync(ct);

        try
        {
            var now = DateTime.UtcNow;
            var today = now.Date;

            if (_lastReset < today)
            {
                _analysisCount = 0;
                _lastReset = today;
            }

            if (_analysisCount >= MaxAnalysisCount)
            {
                return new StockQuoteResult(
                    $"You've reached the daily limit of {MaxAnalysisCount} stock analyses. Please try again tomorrow.",
                    null);
            }

            var cooldown = TimeSpan.FromSeconds(DelayBetweenAnalysesSeconds);
            var elapsedSinceLastExecution = now - _lastExecution;

            if (elapsedSinceLastExecution < cooldown)
            {
                var wait = cooldown - elapsedSinceLastExecution;

                return new StockQuoteResult(
                    $"Next stock analysis is available in {wait.Seconds} second(s).",
                    null);
            }

            _analysisCount++;
            _lastExecution = now;
            reservationMade = true;
            analysesLeftAfterReservation = MaxAnalysisCount - _analysisCount;
        }
        finally
        {
            Gate.Release();
        }

        try
        {
            _logger.LogDebug("Received stock command for symbol {StockSymbol}", symbol);

            if (!await _stockScraper.StockSymbolExists(symbol))
            {
                await RefundAnalysisCount();
                reservationMade = false;

                return new StockQuoteResult("Invalid stock symbol. Please provide a valid stock symbol.", null);
            }

            var analysis = await _stockAnalysisGenerator.GenerateAsync(symbol);
            var analysisMessage = CreateAnalysisMessage(analysis);

            return new StockQuoteResult(
                "Number of analyses left: " + analysesLeftAfterReservation,
                analysisMessage);
        }
        catch (Exception ex)
        {
            if (reservationMade)
                await RefundAnalysisCount();

            _logger.LogError(ex, "Error handling stock command");
            return new StockQuoteResult("Failed to analyze stock.", null);
        }
    }

    private static async Task RefundAnalysisCount()
    {
        await Gate.WaitAsync();

        try
        {
            if (_analysisCount > 0)
                _analysisCount--;
        }
        finally
        {
            Gate.Release();
        }
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
