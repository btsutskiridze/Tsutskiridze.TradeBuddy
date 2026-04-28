using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats;

namespace Tsutskiridze.TradeBuddy.Application.Features.Chats.Services;

public class ActiveChatProvider : IActiveChatProvider
{
    private readonly IReadRepository<Chat> _chats;

    public ActiveChatProvider(IReadRepository<Chat> chats)
    {
        _chats = chats;
    }

    public async Task<bool> ExistsAsync(long telegramChatId, CancellationToken ct = default)
    {
        return await _chats.AnyAsync(new ActiveChatIdByTelegramIdSpec(telegramChatId), ct);
    }

    public async Task<Guid> GetIdAsync(long telegramChatId, CancellationToken ct = default)
    {
        return await _chats.FirstOrDefaultAsync(new ActiveChatIdByTelegramIdSpec(telegramChatId), ct)
                     ?? throw new ResourceNotFoundException("Chat isn't activated.");
    }

    public async Task<Chat> GetAsync(long telegramChatId, CancellationToken ct = default)
    {
        return await _chats.FirstOrDefaultAsync(new ActiveChatByTelegramIdSpec(telegramChatId), ct)
                   ?? throw new ResourceNotFoundException("Chat isn't activated.");
    }
}