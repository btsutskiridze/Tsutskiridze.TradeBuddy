using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Chats;

namespace Tsutskiridze.TradeBuddy.Application.Features.Chats.Specifications;

public sealed class ActiveChatIdByTelegramIdSpec : Specification<Chat, Guid, Guid?>
{
    public ActiveChatIdByTelegramIdSpec(long telegramId)
    {
        Query.Select(x => x.Id)
            .Where(x => x.TelegramChatId == telegramId);
    }
}