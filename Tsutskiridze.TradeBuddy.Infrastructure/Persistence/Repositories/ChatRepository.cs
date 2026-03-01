using Microsoft.EntityFrameworkCore;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Core.Repositories;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

public class ChatRepository : EfRepository<Chat>, IChatRepository
{
    public ChatRepository(AppDbContext db) : base(db)
    {
    }

    public async Task<Chat?> GetByTelegramChatId(long telegramChatId, CancellationToken ct = default)
    {
        return await _db.Chats.FirstOrDefaultAsync(x => x.TelegramChatId == telegramChatId, ct);
    }

    public async Task<Chat?> GetByActivationToken(string activationToken, CancellationToken ct = default)
    {
        return await _db.Chats.FirstOrDefaultAsync(x => x.ActivationToken == activationToken, ct);
    }
}