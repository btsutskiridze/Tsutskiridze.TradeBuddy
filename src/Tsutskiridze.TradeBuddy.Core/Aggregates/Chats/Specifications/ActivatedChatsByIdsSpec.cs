using SharedKernel;
using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;

public class ActivatedChatsByIdsSpec : Specification<Chat>
{
    public ActivatedChatsByIdsSpec(IReadOnlyCollection<Guid> chatIds)
    {
        Query.Where(x => chatIds.Contains(x.Id) && x.IsActivated());
    }
}