using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Tsutskiridze.TradeBuddy.Application.Features.TelegramBot;
using Tsutskiridze.TradeBuddy.Core.Constants;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Telegram
{
    public class TelegramCommandsRegisterService : IHostedService
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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var commands = _registry
                .GetHandlers()
                .Where(h => !string.IsNullOrWhiteSpace(h.Command) && h.Command != TelegramCommands.Unknown)
                .Select(h => new BotCommand
                {
                    Command = h.Command!,
                    Description = h.Description
                })
                .ToArray();

            await _bot.DeleteMyCommands(cancellationToken: cancellationToken);

            await _bot.SetMyCommands(commands, cancellationToken: cancellationToken);

            _logger.LogInformation("Registered commands: {Commands}", string.Join(", ", commands.Select(c => c.Command)));
        }

        public Task StopAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

}
