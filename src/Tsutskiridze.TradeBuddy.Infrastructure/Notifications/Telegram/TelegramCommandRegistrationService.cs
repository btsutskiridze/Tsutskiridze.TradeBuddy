using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram
{
    public class TelegramCommandRegistrationService : IHostedService
    {
        private readonly ITelegramBotClient _bot;
        private readonly ILogger<TelegramCommandRegistrationService> _logger;

        public TelegramCommandRegistrationService(ITelegramBotClient bot, ILogger<TelegramCommandRegistrationService> logger)
        {
            _bot = bot;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            var commands = TelegramCommandCatalog.All
                .Select(x => new BotCommand(x.Command[1..].ToLowerInvariant(), x.Description)).ToList();

            await _bot.DeleteMyCommands(cancellationToken: ct);
            await _bot.SetMyCommands(commands, cancellationToken: ct);

            _logger.LogInformation("Registered commands: {Commands}",
                string.Join(", ", commands.Select(c => c.Command)));
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
