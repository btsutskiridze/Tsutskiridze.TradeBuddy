using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;

public class TelegramSender : ITelegramSender
{
    private readonly ITelegramBotClient _client;
    private readonly ILogger<TelegramSender> _logger;

    public TelegramSender(ITelegramBotClient client, ILogger<TelegramSender> logger)
    {
        _client = client;
        _logger = logger;
    }
    //todo: add resilience everywhere it is necessary
    public async Task Send(TelegramOutgoingMessage message, CancellationToken ct = default)
    {
        await _client.SendMessage(message.ChatId, message.Text, cancellationToken: ct);
    }

    public async Task Send(IEnumerable<TelegramOutgoingMessage> messages, CancellationToken ct = default)
    {
        foreach (var message in messages)
        {
            ct.ThrowIfCancellationRequested();
            await Send(message, ct);
        }
    }

    public async Task SendTypingAction(long chatId, CancellationToken ct = default)
    {
        await _client.SendChatAction(chatId, ChatAction.Typing, cancellationToken: ct);
    }
}