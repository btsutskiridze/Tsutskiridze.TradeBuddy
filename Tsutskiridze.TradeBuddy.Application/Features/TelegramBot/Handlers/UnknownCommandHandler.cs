using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers
{
    public class UnknownCommandHandler : ITelegramCommandHandler
    {
        public string Command => "unknown_command";
        public string Pattern => string.Empty;
        public string Description => "Unknown command";

        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<UnknownCommandHandler> _logger;

        public UnknownCommandHandler(ITelegramBotClient botClient, ILogger<UnknownCommandHandler> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleMessage(Message message)
        {
            _logger.LogDebug("Received unknown command '{Command}' from chat {ChatId}", message.Text, message.Chat.Id);
            await _botClient.SendMessage(
                message.Chat.Id,
                "Unknown command"
            );
        }
    }
}
