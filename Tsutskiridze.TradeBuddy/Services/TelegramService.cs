using Telegram.Bot;

namespace Tsutskiridze.TradeBuddy.Services
{
    public class TelegramService
    {
        private static readonly string _groupChatID = SecretsManager.GetSecret("Telegram:GroupChatID");

        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramService> _logger;
        public TelegramService(ILogger<TelegramService> logger, ITelegramBotClient botClient)
        {
            _logger = logger;
            _botClient = botClient;
        }

        public ITelegramBotClient GetBotClient()
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
