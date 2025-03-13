using Telegram.Bot;

namespace Tsutskiridze.TradeBuddy.Services
{
    public class TelegramService
    {
        private static readonly string _botToken = SecretsManager.GetSecret("Telegram:BotToken");
        private static readonly string _groupChatID = SecretsManager.GetSecret("Telegram:GroupChatID");

        private readonly TelegramBotClient _botClient;
        private readonly ILogger<TelegramService> _logger;
        public TelegramService(ILogger<TelegramService> logger)
        {
            _logger = logger;
            _botClient = new TelegramBotClient(_botToken);
        }

        public TelegramBotClient GetBotClient()
        {
            return _botClient;
        }

        public async Task SendMessage(string message)
        {
            try
            {
                _logger.LogInformation("Sending message to Telegram");
                await _botClient.SendMessage(_groupChatID, message);

                _logger.LogInformation("Message sent to Telegram");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to Telegram");
            }
        }
    }
}
