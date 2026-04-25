using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats.Specifications;

public sealed class ActivatedChatsByIdsSpec : Specification<Chat>
{
    public ActivatedChatsByIdsSpec(IReadOnlyCollection<Guid> chatIds)
    {
        Query.Where(x => chatIds.Contains(x.Id) && x.TelegramChatId.HasValue);
    }
}