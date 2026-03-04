using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;

public class ActivatedChatsByIdsSpec : Specification<Chat>
{
    public ActivatedChatsByIdsSpec(IReadOnlyCollection<Guid> chatIds)
    {
        Query(query => query.Where(x => chatIds.Contains(x.Id) && x.IsActivated()));
    }
}