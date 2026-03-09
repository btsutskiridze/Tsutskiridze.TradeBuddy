using Mediator;
using Telegram.Bot.Types;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;

public class TelegramUpdateRouter : ITelegramUpdateRouter
{
    private readonly IMediator _mediator;

    public TelegramUpdateRouter(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task RouteAsync(TelegramUpdateDto updateDto, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(updateDto.Text))
            return;

        var text = updateDto.Text.Trim();
        
        if (!text.StartsWith('/'))
            return;

        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].ToLowerInvariant();

        switch (command)
        {
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