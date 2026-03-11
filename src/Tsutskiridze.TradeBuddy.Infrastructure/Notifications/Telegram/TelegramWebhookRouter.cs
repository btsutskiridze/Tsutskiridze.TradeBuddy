using Mediator;
using SharedKernel;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Commands;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;

public class TelegramWebhookRouter : ITelegramWebhookRouter
{
    private readonly IMediator _mediator;

    public TelegramWebhookRouter(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<TelegramUpdateResultDto?> RouteAsync(TelegramUpdateDto update, CancellationToken ct)
    {
        var text = ExtractText(update);
        if(text is null) return null;
        
        var (command, parts) = ExtractParts(text);

        try
        {
            await ExecuteHandler(update.ChatId, command, parts, ct);
            return null;
        }
        catch (BaseException exception)
        {
            return new TelegramUpdateResultDto()
            {
                ChatId = update.ChatId,
                Text = exception.Message
            };
        }
    }

    private static (string, string[]) ExtractParts(string text)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].ToLowerInvariant();
        
        return (command, parts.Skip(1).ToArray());
    }

    private string? ExtractText(TelegramUpdateDto updateDto)
    {
        var text = updateDto.Text?.Trim();
        
        if (string.IsNullOrEmpty(text) || !text.StartsWith('/'))
        {
            return null;
        }

        return text;
    }


    private async Task ExecuteHandler(long chatId, string command, string[] parts, CancellationToken ct)
    {
        switch (command)
        {
            case "/activate":
                await _mediator.Send(new ActivateBotCommand(chatId, parts[0]), ct);
                break;
            // case "/start":
            //     await _mediator.Send(new StartCommand(
            //         update.Message.Chat.Id), ct);
            //     break;
            //
            // case "/alert":
            //     await _mediator.Send(new CreatePriceAlertCommand(
            //         update.Message.Chat.Id,
            //         parts[1],
            //         decimal.Parse(parts[2])), ct);
            //     break;
            //
            // case "/alerts":
            //     await _mediator.Send(new ListAlertsQuery(
            //         update.Message.Chat.Id), ct);
            //     break;
        }
    }
}