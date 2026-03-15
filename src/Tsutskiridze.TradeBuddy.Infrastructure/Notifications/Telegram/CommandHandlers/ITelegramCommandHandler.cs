using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.CommandHandlers;

public interface ITelegramCommandHandler
{
    string Command { get; }

    Task Handle(TelegramUpdateDto update, CancellationToken ct);
}