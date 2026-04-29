using Mediator;
using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats.Specifications;

namespace Tsutskiridze.TradeBuddy.Application.Features.Chats.Commands.ActivateChat;

public sealed record ActivateChatCommand(long ChatId, string Token) : ICommand;

public sealed class ActivateChatHandler : ICommandHandler<ActivateChatCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<Chat> _repo;

    public ActivateChatHandler(IUnitOfWork uow, IRepository<Chat> repo)
    {
        _uow = uow;
        _repo = repo;
    }

    public async ValueTask<Unit> Handle(ActivateChatCommand command,
        CancellationToken cancellationToken)
    {
        var chat = await _repo.FirstOrDefaultAsync(new ChatByActivationTokenSpec(command.Token), cancellationToken)
                   ?? throw new ResourceNotFoundException("Invalid activation token");

        chat.Activate(command.ChatId);
        await _uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
