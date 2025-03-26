using Telegram.Bot;
using Telegram.Bot.Types;
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

                _analysisCount++;
                _lastExecution = DateTime.UtcNow;
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

            // Proceed with actual logic
            string stockSymbol = message.Text!.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();
            _logger.LogInformation("Received stock command for symbol {StockSymbol}", stockSymbol);

            var analysis = await _stockService.GenerateAiStockAnalysis(stockSymbol);
            await _stockService.SendStockAnalysisToTelegram(message.Chat.Id, analysis);

            await _botClient.SendMessage(message.Chat.Id, "Number of analyses left: " + (20 - _analysisCount));
        }

    }
}
