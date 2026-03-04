using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;

public class ChatByTelegramId : Specification<Chat>
{
    public ChatByTelegramId(long telegramId)
    {
        Query(query => query.Where(x => x.TelegramChatId == telegramId));
    }
}