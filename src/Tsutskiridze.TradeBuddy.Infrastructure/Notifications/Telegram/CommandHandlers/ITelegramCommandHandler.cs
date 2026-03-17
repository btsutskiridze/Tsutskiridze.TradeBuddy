using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.CommandHandlers;

public interface ITelegramCommandHandler
{
    string Command { get; }
    string Description { get; }

    Task<TelegramUpdateResultDto?> Handle(TelegramUpdateDto update, CancellationToken ct);
}