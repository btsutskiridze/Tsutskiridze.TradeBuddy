using SharedKernel;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;

namespace Tsutskiridze.TradeBuddy.Core.Repositories;

public interface IChatRepository : IRepository<Chat>
{
    Task<Chat?> GetByTelegramChatId(long telegramChatId, CancellationToken ct = default);
    Task<Chat?> GetByActivationToken(string activationToken, CancellationToken ct = default);
}