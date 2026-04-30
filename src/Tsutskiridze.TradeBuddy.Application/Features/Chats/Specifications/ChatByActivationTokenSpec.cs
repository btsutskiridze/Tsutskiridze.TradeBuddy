using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Chats;

namespace Tsutskiridze.TradeBuddy.Application.Features.Chats.Specifications;

public sealed class ChatByActivationTokenSpec : Specification<Chat>
{
    public ChatByActivationTokenSpec(string activationToken)
    {
        Query.Where(x => x.ActivationToken == activationToken);
    }
}