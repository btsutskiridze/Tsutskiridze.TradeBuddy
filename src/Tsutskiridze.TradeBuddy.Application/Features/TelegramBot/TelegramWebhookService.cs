using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Options;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Core.Constants;
using Chat = Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Chat;

namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot
{
    public class TelegramWebhookService
    {
        private readonly TelegramBotOptions _options;
        private readonly ILogger<TelegramWebhookService> _logger;
        private readonly ITelegramHandlerRegistry _handlerRegistry;
        private readonly IServiceScopeFactory _serviceScope;

        public TelegramWebhookService(
            ILogger<TelegramWebhookService> logger,
            ITelegramHandlerRegistry handlerRegistry,
            IOptions<TelegramBotOptions> options,
            IServiceScopeFactory serviceScopeFactory
        )
        {
            _logger = logger;
            _handlerRegistry = handlerRegistry;
            _options = options.Value;
            _serviceScope = serviceScopeFactory;
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
            using var scope = _serviceScope.CreateScope();
            var chatRepo = scope.ServiceProvider.GetRequiredService<IReadRepository<Chat>>();
            var telegramClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

            if (message.Text == null)
            {
                _logger.LogInformation("Received message with no text from chat {ChatId}", message.Chat.Id);
                return;
            }

            _logger.LogInformation("Received message '{MessageText}' from chat {ChatId}", message.Text,
                message.Chat.Id);

            var (command, cleanText) = ParseMessage(message.Text);

            var chat = await chatRepo.FirstOrDefaultAsync(
                new ChatByTelegramIdSpec(message.Chat.Id)
            );

            if (chat == null && command != TelegramCommands.ActivateBot)
            {
                _logger.LogDebug("Activated Chat {ChatId} not found", message.Chat.Id);
                await telegramClient.SendMessage(
                    message.Chat.Id,
                    "This chat is not activated. Please use the activation command to start using the bot."
                );
                return;
            }

            var handler = _handlerRegistry.GetHandler(command);
            message.Text = cleanText;

            await handler.HandleMessage(message);

            _logger.LogInformation("Message handled by {Handler}", handler.GetType().Name);
        }

        private static (string command, string cleanText) ParseMessage(string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                return (string.Empty, string.Empty);
            }

            var parts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return (string.Empty, messageText);
            }

            var firstWord = parts[0].TrimStart('/').ToLowerInvariant();
            var commandParts = firstWord.Split('@');
            var command = commandParts[0];
            var botUsername = commandParts.Length > 1 ? commandParts[1] : string.Empty;

            var cleanText = string.IsNullOrEmpty(botUsername)
                ? messageText
                : messageText.Replace($"@{botUsername}", string.Empty, StringComparison.OrdinalIgnoreCase);

            return (command, cleanText);
        }
    }
}