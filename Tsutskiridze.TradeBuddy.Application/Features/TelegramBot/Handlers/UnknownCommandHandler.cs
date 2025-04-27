using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Tsutskiridze.TradeBuddy.Core.Constants;

namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers
{
    public class UnknownCommandHandler : ITelegramCommandHandler
    {
        public string Command => TelegramCommands.Unknown;
        public string Description => "Unknown command. Please use /help to see the list of available commands.";

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
