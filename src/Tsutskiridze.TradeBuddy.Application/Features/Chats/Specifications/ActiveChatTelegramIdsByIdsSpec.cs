using SharedKernel.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Chats;

namespace Tsutskiridze.TradeBuddy.Application.Features.Chats.Specifications;

internal sealed record ChatTelegramIdSpecResult(Guid ChatId, long TelegramId);

internal sealed class ActiveChatTelegramIdsByIdsSpec : Specification<Chat, Guid, ChatTelegramIdSpecResult>
{
    public ActiveChatTelegramIdsByIdsSpec(IReadOnlyCollection<Guid> chatIds)
    {
        Query
            .Select(x => new ChatTelegramIdSpecResult(x.Id, x.TelegramChatId!.Value))
            .Where(x => chatIds.Contains(x.Id) && x.TelegramChatId.HasValue);
    }
}