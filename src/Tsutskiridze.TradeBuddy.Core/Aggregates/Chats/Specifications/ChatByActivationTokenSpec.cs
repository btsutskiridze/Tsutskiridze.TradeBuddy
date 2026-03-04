using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;

public class ChatByActivationTokenSpec : Specification<Chat>
{
    public ChatByActivationTokenSpec(string activationToken)
    {
        Query(query => query.Where(x => x.ActivationToken == activationToken));
    }
}