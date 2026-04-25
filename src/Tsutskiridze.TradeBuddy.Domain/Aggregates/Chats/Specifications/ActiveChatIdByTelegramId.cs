using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats.Specifications;

public sealed class ActiveChatIdByTelegramId : Specification<Chat, Guid?>
{
    public ActiveChatIdByTelegramId(long telegramId)
    {
        Query.Select(x => x.Id)
            .Where(x => x.TelegramChatId == telegramId && x.TelegramChatId.HasValue);
    }
}