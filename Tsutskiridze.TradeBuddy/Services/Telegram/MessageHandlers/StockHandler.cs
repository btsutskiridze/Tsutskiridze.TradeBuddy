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
                    return;
                }

                if ((DateTime.UtcNow - _lastExecution) < TimeSpan.FromMinutes(5))
                {
                    isCoolingDown = true;
                    return;
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
                await _botClient.SendMessage(
                    message.Chat.Id,
                    $"Next stock analysis is available in {TimeSpan.FromMinutes(5) - (DateTime.UtcNow - _lastExecution)}.");
                return;
            }

            try
            {
                await _botClient.SendChatAction(message.Chat.Id, ChatAction.Typing);

                await ValidateSymbol(message);

                string stockSymbol = message.Text!.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();

                _logger.LogInformation("Received stock command for symbol {StockSymbol}", stockSymbol);

                var analysis = await _stockService.GenerateAiStockAnalysis(stockSymbol);

                lock (_lock)
                {
                    _analysisCount++;
                    _lastExecution = DateTime.UtcNow;
                }

                await _stockService.SendStockAnalysisToTelegram(message.Chat.Id, analysis);

                await _botClient.SendMessage(message.Chat.Id, "Number of analyses left: " + (20 - _analysisCount));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling stock command");
                await _botClient.SendMessage(message.Chat.Id, "Failed to analyze stock.");
            }
        }

        private async Task ValidateSymbol(Message message)
        {
            string stockSymbol = message.Text!.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();

            if (stockSymbol == Command)
            {
                await _botClient.SendMessage(message.Chat.Id, "Please provide a stock symbol.");
            }

            if (stockSymbol.Length < 2)
            {
                await _botClient.SendMessage(message.Chat.Id, "Stock symbol is too short.");
                return;
            }

            if (string.IsNullOrWhiteSpace(stockSymbol))
            {
                await _botClient.SendMessage(message.Chat.Id, "Please provide a stock symbol.");
                return;
            }

            if (stockSymbol.Length > 10)
            {
                await _botClient.SendMessage(message.Chat.Id, "Stock symbol is too long.");
                return;
            }

            if (stockSymbol.Any(char.IsDigit))
            {
                await _botClient.SendMessage(message.Chat.Id, "Stock symbol cannot contain digits.");
                return;
            }

            if (stockSymbol.Any(char.IsWhiteSpace))
            {
                await _botClient.SendMessage(message.Chat.Id, "Stock symbol cannot contain whitespace.");
                return;
            }
        }

    }
}
