using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram
{
    public class TelegramCommandsRegisterService : BackgroundService
    {
        private readonly ITelegramBotClient _bot;
        private readonly ITelegramHandlerRegistry _registry;
        private readonly ILogger<TelegramCommandsRegisterService> _logger;

        public TelegramCommandsRegisterService(
            ITelegramBotClient bot,
            ITelegramHandlerRegistry registry,
            ILogger<TelegramCommandsRegisterService> logger
        )
        {
            _bot = bot;
            _registry = registry;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var commands = _registry
                .GetHandlers()
                .Where(h => !string.IsNullOrWhiteSpace(h.Command) && typeof(UnknownCommandHandler) != h.GetType())
                .Select(h => new BotCommand
                {
                    Command = h.Command!,
                    Description = string.IsNullOrEmpty(h.Pattern) ? h.Description : h.Pattern
                })
                .ToArray();

            await _bot.DeleteMyCommands(cancellationToken: ct);

            await _bot.SetMyCommands(commands, cancellationToken: ct);

            _logger.LogInformation("Registered commands: {Commands}", string.Join(", ", commands.Select(c => c.Command)));
        }
    }

}
