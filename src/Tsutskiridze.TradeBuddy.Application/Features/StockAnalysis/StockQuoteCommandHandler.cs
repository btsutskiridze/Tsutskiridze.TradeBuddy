using Mediator;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.DTOs.StockAnalysis;

namespace Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis;

public record StockQuoteCommand(long ChatId, string Symbol) : ICommand<TelegramUpdateResultDto?>;

public class StockQuoteCommandHandler : ICommandHandler<StockQuoteCommand, TelegramUpdateResultDto?>
{
    private readonly ILogger<StockQuoteCommandHandler> _logger;
    private readonly StockAnalysisService _stockService;
    private readonly ITelegramSender _sender;
    private readonly IYahooMarketDataProvider _yahooStockScraper;

    private static int _analysisCount = 0;
    private static DateTime _lastReset = DateTime.UtcNow.Date;
    private static DateTime _lastExecution = DateTime.MinValue;
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private const int MaxAnalysisCount = 100;
    private const int DelayBetweenAnalysesSeconds = 5;

    public StockQuoteCommandHandler(
        ILogger<StockQuoteCommandHandler> logger,
        StockAnalysisService stockService,
        ITelegramSender sender,
        IYahooMarketDataProvider yahooStockScraper)
    {
        _logger = logger;
        _stockService = stockService;
        _sender = sender;
        _yahooStockScraper = yahooStockScraper;
    }

    public async ValueTask<TelegramUpdateResultDto?> Handle(StockQuoteCommand command,
        CancellationToken ct)
    {
        var chatId = command.ChatId;
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
                return new TelegramUpdateResultDto()
                {
                    ChatId = chatId,
                    Text =
                        $"You've reached the daily limit of {MaxAnalysisCount} stock analyses. Please try again tomorrow."
                };
            }

            var cooldown = TimeSpan.FromSeconds(DelayBetweenAnalysesSeconds);
            var elapsedSinceLastExecution = now - _lastExecution;

            if (elapsedSinceLastExecution < cooldown)
            {
                var wait = cooldown - elapsedSinceLastExecution;

                return new TelegramUpdateResultDto
                {
                    ChatId = chatId,
                    Text = $"Next stock analysis is available in {wait.Seconds} second(s)."
                };
            }

            // Reserve the slot inside the same critical section.
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

            if (!await _yahooStockScraper.StockSymbolExists(symbol))
            {
                await RefundAnalysisCount();
                reservationMade = false;

                return new TelegramUpdateResultDto
                {
                    ChatId = chatId,
                    Text = "Invalid stock symbol. Please provide a valid stock symbol."
                };
            }

            var analysis = await _stockService.GenerateAiStockAnalysis(symbol);

            await _sender.SendMessage(chatId, CreateAnalysisMessage(analysis), ct);

            return new TelegramUpdateResultDto()
            {
                ChatId = chatId,
                Text = "Number of analyses left: " + (analysesLeftAfterReservation)
            };
        }
        catch (Exception ex)
        {
            if (reservationMade)
                await RefundAnalysisCount();

            _logger.LogError(ex, "Error handling stock command");
            return new TelegramUpdateResultDto()
            {
                ChatId = chatId,
                Text = "Failed to analyze stock."
            };
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

    private static string CreateAnalysisMessage(StockAnalysisResultDto analysis)
    {
        // Build the message using string interpolation
        var message = $"🚨 Stock Alert: {analysis.Symbol} 🚨\n" +
                      $"- 📈 Current Price: {analysis.price}\n" +
                      $"- 📊 50-day Avg: {analysis.bench.avg50} | Year High: {analysis.bench.yearHigh}\n" +
                      $"- 🔔 Trading Volume: {analysis.volAnalysis}\n" +
                      $"- 📰 Overall News: {analysis.newsOverall.conf} Positive\n" +
                      $"- 🤖 AI Analysis: {analysis.ai.rec} ({analysis.ai.conf} Confidence)\n" +
                      $"- ⏱️ Analysis Duration: {analysis.ExecutionTime:0.00}s";

        return message;
    }
}