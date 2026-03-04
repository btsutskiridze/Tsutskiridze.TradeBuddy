using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;

public class ChatByTelegramIdIncludePriceAlertsSpec : Specification<Chat>
{
    public ChatByTelegramIdIncludePriceAlertsSpec(long telegramId)
    {
        Query(query => query
            .Where(x => x.TelegramChatId == telegramId));
    }
}