using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Options;

namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot
{
    public class TelegramWebhookService
    {
        private readonly TelegramBotOptions _options;
        private readonly ILogger<TelegramWebhookService> _logger;
        private readonly ITelegramHandlerRegistry _handlerRegistry;

        public TelegramWebhookService(
            ILogger<TelegramWebhookService> logger,
            ITelegramHandlerRegistry handlerRegistry,
            IOptions<TelegramBotOptions> options
        )
        {
            _logger = logger;
            _handlerRegistry = handlerRegistry;
            _options = options.Value;
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

            if (message.Chat.Id != _options.GroupChatID)
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
            var parts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return string.Empty;
            }

            string command = parts[0].TrimStart('/').ToLowerInvariant();
            return command;
        }

    }
}
