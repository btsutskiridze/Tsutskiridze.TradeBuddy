using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Chats;

namespace Tsutskiridze.TradeBuddy.Application.Features.Chats.Specifications;

public sealed class ActiveChatByTelegramIdSpec : Specification<Chat>
{
    public ActiveChatByTelegramIdSpec(long telegramId)
    {
        Query.Where(x => x.TelegramChatId == telegramId);
    }
}