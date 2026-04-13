using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.Contracts;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram.CommandHandlers;

public interface ITelegramCommandHandler
{
    string Command { get; }
    string Description { get; }

    Task<TelegramMessageResponse?> Handle(TelegramMessageRequest update, CancellationToken ct);
}
