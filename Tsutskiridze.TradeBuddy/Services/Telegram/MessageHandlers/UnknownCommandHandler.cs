using Telegram.Bot;
using Telegram.Bot.Types;
using Tsutskiridze.TradeBuddy.Helpers;

namespace Tsutskiridze.TradeBuddy.Services.Telegram.MessageHandlers
{
    public class UnknownCommandHandler : ITelegramMessageHandler
    {
        public string Command => TelegramCommands.Unknown;

        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<UnknownCommandHandler> _logger;

        public UnknownCommandHandler(ITelegramBotClient botClient, ILogger<UnknownCommandHandler> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleMessage(Message message)
        {
            _logger.LogInformation("Received unknown command '{Command}' from chat {ChatId}", message.Text, message.Chat.Id);

            await _botClient.SendMessage(
                message.Chat.Id,
                "Unknown command"
            );
        }
    }
}
