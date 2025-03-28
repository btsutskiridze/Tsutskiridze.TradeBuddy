using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Helpers;

namespace Tsutskiridze.TradeBuddy.Services.Telegram.MessageHandlers
{

    public class StockHandler : ITelegramMessageHandler
    {
        public string Command => TelegramCommands.Stock;

        private readonly ILogger<StockHandler> _logger;
        private readonly StockAnalysisService _stockService;
        private readonly ITelegramBotClient _botClient;

        private static int _analysisCount = 0;
        private static DateTime _lastReset = DateTime.UtcNow.Date;
        private static DateTime _lastExecution = DateTime.MinValue;
        private static readonly object _lock = new();

        public StockHandler(ILogger<StockHandler> logger, StockAnalysisService stockService, ITelegramBotClient botClient)
        {
            _logger = logger;
            _stockService = stockService;
            _botClient = botClient;
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

                if (_analysisCount >= 20)
                {
                    isRateLimited = true;
                }

                if ((DateTime.UtcNow - _lastExecution) < TimeSpan.FromMinutes(5))
                {
                    isCoolingDown = true;
                }
            }

            if (isRateLimited)
            {
                await _botClient.SendMessage(
                    message.Chat.Id,
                    "You've reached the daily limit of 20 stock analyses. Please try again tomorrow.");
                return;
            }

            if (isCoolingDown)
            {
                var nextExecutionIn = TimeSpan.FromMinutes(5) - (DateTime.UtcNow - _lastExecution);
                string formattedTime = nextExecutionIn.ToString("mm\\:ss");

                await _botClient.SendMessage(
                    message.Chat.Id,
                    $"Next stock analysis is available in {formattedTime} minutes.");
                return;
            }

            try
            {
                await _botClient.SendChatAction(message.Chat.Id, ChatAction.Typing);

                string stockSymbol = message.Text!.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();

                if (!IsValidStockSymbol(stockSymbol))
                {
                    await _botClient.SendMessage(message.Chat.Id, "Invalid stock symbol. Please provide a valid stock symbol.");
                    return;
                }


                _logger.LogInformation("Received stock command for symbol {StockSymbol}", stockSymbol);

                lock (_lock)
                {
                    _analysisCount++;
                    _lastExecution = DateTime.UtcNow;
                }

                var analysis = await _stockService.GenerateAiStockAnalysis(stockSymbol);

                await _stockService.SendStockAnalysisToTelegram(message.Chat.Id, analysis);

                await _botClient.SendMessage(message.Chat.Id, "Number of analyses left: " + (20 - _analysisCount));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling stock command");
                await _botClient.SendMessage(message.Chat.Id, "Failed to analyze stock.");
            }
        }

        public bool IsValidStockSymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol) || symbol == Command)
                return false;

            symbol = symbol.Trim().ToUpperInvariant();

            // Basic pattern: 1 to 10 uppercase letters, optionally followed by a dot and a class (e.g., BRK.A)
            var regex = new Regex(@"^([A-Z]{1,5}(\.[A-Z]{1,2})?|[A-Z]{2,}:[A-Z0-9]+(\s[A-Z])?)$");
            return regex.IsMatch(symbol);
        }

    }
}
