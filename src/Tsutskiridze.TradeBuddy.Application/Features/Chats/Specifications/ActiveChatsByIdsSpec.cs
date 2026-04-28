using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats;

namespace Tsutskiridze.TradeBuddy.Application.Features.Chats.Specifications;

public sealed class ActiveChatsByIdsSpec : Specification<Chat>
{
    public ActiveChatsByIdsSpec(IReadOnlyCollection<Guid> chatIds)
    {
        Query.Where(x => chatIds.Contains(x.Id) && x.TelegramChatId.HasValue);
    }
}