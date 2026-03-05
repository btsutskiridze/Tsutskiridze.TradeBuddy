using SharedKernel;
using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;

public class ChatByTelegramId : Specification<Chat>
{
    public ChatByTelegramId(long telegramId)
    {
        Query.Where(x => x.TelegramChatId == telegramId);
    }
}