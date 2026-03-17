using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;

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

    public async Task SendMessage(long chatId, string text, CancellationToken ct = default)
    {
        try
        {
            await _client.SendMessage(
                chatId,
                text,
                cancellationToken: ct
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            ExceptionDispatchInfo.Throw(e);
        }
    }

    public async Task SendTypingAction(long chatId, CancellationToken ct = default)
    {
        try
        {
            await _client.SendChatAction(chatId, ChatAction.Typing, cancellationToken: ct);
        }
        catch(Exception e)
        {
            _logger.LogWarning(e.Message);
        }
    }
}