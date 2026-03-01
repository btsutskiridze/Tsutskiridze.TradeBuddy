using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using Telegram.Bot;
using Telegram.Bot.Types;
using Tsutskiridze.TradeBuddy.Core.Constants;
using Chat = Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Chat;


namespace Tsutskiridze.TradeBuddy.Application.Features.TelegramBot.Handlers
{
    public class ActivateBotCommandHandler : ITelegramCommandHandler
    {
        public string Command => TelegramCommands.ActivateBot;
        public string Pattern => "<activation-token>";
        public string Description => $"Activate the bot. e.g: /{Command} 1234567890";

        private readonly ITelegramBotClient _telegramClient;
        private readonly IServiceScopeFactory _scopes;

        public ActivateBotCommandHandler(ITelegramBotClient telegramClient, IServiceScopeFactory scopes)
        {
            _telegramClient = telegramClient;
            _scopes = scopes;
        }

        public async Task HandleMessage(Message message)
        {
            using var scope = _scopes.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var chatReadRepository = scope.ServiceProvider.GetRequiredService<IRepository<Chat>>();
            
            var parts = message.Text!
                              .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                await _telegramClient.SendMessage(message.Chat.Id, "Invalid command. Please use the format: /activatebot <activation-token>");
                return;
            }

            var activationToken = parts[1];

            var chat = await chatReadRepository.FirstOrDefaultAsync(c => c.ActivationToken == activationToken);

            await _telegramClient.SendChatAction(message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);

            if (chat == null)
            {
                await _telegramClient.SendMessage(message.Chat.Id, "Invalid activation token. Please check and try again.");
                return;
            }

            if (chat.IsActivated())
            {
                await _telegramClient.SendMessage(message.Chat.Id, "Activation already completed for this token.");
                return;
            }

            chat.Activate(message.Chat.Id);
            await unitOfWork.SaveChangesAsync();

            await _telegramClient.SendMessage(message.Chat.Id, "Bot activated successfully! You can now use the bot features.");
        }
    }
}
