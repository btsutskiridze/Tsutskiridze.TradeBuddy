using SharedKernel;
using SharedKernel.Specifications;

namespace Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;

public class ChatByActivationTokenSpec : Specification<Chat>
{
    public ChatByActivationTokenSpec(string activationToken)
    {
        Query.Where(x => x.ActivationToken == activationToken);
    }
}