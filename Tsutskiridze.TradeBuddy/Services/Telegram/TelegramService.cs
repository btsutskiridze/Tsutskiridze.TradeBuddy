using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Tsutskiridze.TradeBuddy.Services.Telegram
{
    public class TelegramService
    {
        private readonly ILogger<TelegramService> _logger;
        private readonly ITelegramHandlerRegistry _handlerRegistry;

        public TelegramService(
            ILogger<TelegramService> logger,
            ITelegramHandlerRegistry handlerRegistry
        )
        {
            _logger = logger;
            _handlerRegistry = handlerRegistry;
        }

        public async Task HandleUpdate(Update update)
        {
            if (update.Type == UpdateType.Message && update.Message != null)
            {
                await HandleMessage(update.Message);
            }
        }

        private async Task HandleMessage(Message message)
        {
            if (message.Text == null)
            {
                _logger.LogInformation("Received message with no text from chat {ChatId}", message.Chat.Id);
                return;
            }

            if (message.Chat.Id != long.Parse(SecretsManager.GetSecret("Telegram:GroupChatID")))
            {
                _logger.LogInformation("Received message from chat {ChatId} that is not the group chat", message.Chat.Id);
                return;
            }

            _logger.LogInformation("Received message '{MessageText}' from chat {ChatId}", message.Text, message.Chat.Id);

            string command = GetCommand(message.Text);

            var handler = _handlerRegistry.GetHandler(command);

            await handler.HandleMessage(message);

            _logger.LogInformation("Message handled by {Handler}", handler.GetType().Name);
        }


        private static string GetCommand(string messageText)
        {
            return messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries).First();
        }

    }
}
