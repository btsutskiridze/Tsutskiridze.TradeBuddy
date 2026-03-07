using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis;
using Tsutskiridze.TradeBuddy.Core.Constants;

namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers
{

    public class QuoteCommandHandler : ITelegramCommandHandler
    {
        public string Command => TelegramCommands.Quote;
        public string Pattern => "<symbol>";
        public string Description => $"Get stock analysis for a symbol. e.g: /{Command} NVDA";

        private readonly ILogger<QuoteCommandHandler> _logger;
        private readonly StockAnalysisService _stockService;
        private readonly ITelegramBotClient _botClient;
        private readonly IYahooMarketDataProvider _yahooStockScraper;

        private static int _analysisCount = 0;
        private static DateTime _lastReset = DateTime.UtcNow.Date;
        private static DateTime _lastExecution = DateTime.MinValue;
        private static readonly object _lock = new();

        private const int MaxAnalysisCount = 100;
        private const int DelayBetweenAnalyses = 5;

        public QuoteCommandHandler(ILogger<QuoteCommandHandler> logger, StockAnalysisService stockService, ITelegramBotClient botClient, IYahooMarketDataProvider yahooStockScraper)
        {
            _logger = logger;
            _stockService = stockService;
            _botClient = botClient;
            _yahooStockScraper = yahooStockScraper;
        }

        public async Task HandleMessage(Message message)
        {
            bool isRateLimited = false;
            bool isCoolingDown = false;

            lock (_lock)
            {
                if (_lastReset.Date < DateTime.UtcNow.Date)
                {
                    _analysisCount = 0;
                    _lastReset = DateTime.UtcNow.Date;
                }

                if (_analysisCount >= MaxAnalysisCount)
                {
                    isRateLimited = true;
                }

                if (DateTime.UtcNow - _lastExecution < TimeSpan.FromSeconds(DelayBetweenAnalyses))
                {
                    isCoolingDown = true;
                }
            }

            if (isRateLimited)
            {
                await _botClient.SendMessage(
                    message.Chat.Id,
                    $"You've reached the daily limit of {MaxAnalysisCount} stock analyses. Please try again tomorrow.");
                return;
            }

            if (isCoolingDown)
            {
                var nextExecutionIn = TimeSpan.FromSeconds(DelayBetweenAnalyses) - (DateTime.UtcNow - _lastExecution);
                string formattedTime = nextExecutionIn.ToString("mm\\:ss");

                await _botClient.SendMessage(
                    message.Chat.Id,
                    $"Next stock analysis is available in {formattedTime} minutes.");
                return;
            }

            try
            {
                await _botClient.SendChatAction(message.Chat.Id, ChatAction.Typing);


                string? stockSymbol = GetCommandParameter(message.Text);

                if (string.IsNullOrWhiteSpace(stockSymbol))
                {
                    await _botClient.SendMessage(message.Chat.Id, $"e.g: /{Command} NVDA");
                    return;
                }

                if (!await _yahooStockScraper.StockSymbolExits(stockSymbol))
                {
                    await _botClient.SendMessage(message.Chat.Id, "Invalid stock symbol. Please provide a valid stock symbol.");
                    return;
                }


                _logger.LogDebug("Received stock command for symbol {StockSymbol}", stockSymbol);

                lock (_lock)
                {
                    _analysisCount++;
                    _lastExecution = DateTime.UtcNow;
                }

                var analysis = await _stockService.GenerateAiStockAnalysis(stockSymbol);

                await _stockService.SendStockAnalysisToTelegram(message.Chat.Id, analysis);

                await _botClient.SendMessage(message.Chat.Id, "Number of analyses left: " + (MaxAnalysisCount - _analysisCount));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling stock command");
                await _botClient.SendMessage(message.Chat.Id, "Failed to analyze stock.");
            }
        }

        private string? GetCommandParameter(string? command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return null;
            }

            var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? parts[1].ToUpperInvariant() : null;
        }

    }
}
