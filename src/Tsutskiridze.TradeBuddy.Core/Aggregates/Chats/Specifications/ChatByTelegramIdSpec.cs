using SharedKernel;
using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;

public sealed class ChatByTelegramIdSpec : Specification<Chat>
{
    public ChatByTelegramIdSpec(long telegramId)
    {
        Query.Where(x => x.TelegramChatId == telegramId);
    }
}