using SharedKernel;
using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;

public class ChatByTelegramIdIncludePriceAlertsSpec : Specification<Chat>
{
    public ChatByTelegramIdIncludePriceAlertsSpec(long telegramId)
    {
        Query.Where(x => x.TelegramChatId == telegramId)
            .Include(x => x.PriceAlerts);
    }
}