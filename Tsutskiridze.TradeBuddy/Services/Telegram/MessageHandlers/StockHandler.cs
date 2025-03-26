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
            lock (_lock)
            {
                // Reset daily counter if day has changed
                if (_lastReset.Date < DateTime.UtcNow.Date)
                {
                    _analysisCount = 0;
                    _lastReset = DateTime.UtcNow.Date;
                }

                // Check daily limit
                if (_analysisCount >= 20)
                {
                    _logger.LogInformation("Daily analysis limit reached");
                    return;
                }

                // Check cooldown
                if ((DateTime.UtcNow - _lastExecution) < TimeSpan.FromMinutes(5))
                {
                    _logger.LogInformation("Cooldown in effect, try again later");
                    return;
                }

                _analysisCount++;
                _lastExecution = DateTime.UtcNow;
            }

            string stockSymbol = message.Text!.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();
            _logger.LogInformation("Received stock command for symbol {StockSymbol}", stockSymbol);

            var analysis = await _stockService.GenerateAiStockAnalysis(stockSymbol);
            await _stockService.SendStockAnalysisToTelegram(message.Chat.Id, analysis);
        }
    }
}
