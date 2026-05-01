using Tsutskiridze.TradeBuddy.Domain.Chats;

namespace Tsutskiridze.TradeBuddy.Application.Features.Chats.Services;

public interface IActiveChatProvider
{
    Task<bool> ExistsAsync(long telegramChatId, CancellationToken ct = default);
    Task<Guid> GetIdAsync(long telegramChatId, CancellationToken ct = default);
    Task<Chat> GetAsync(long telegramChatId, CancellationToken ct = default);
}