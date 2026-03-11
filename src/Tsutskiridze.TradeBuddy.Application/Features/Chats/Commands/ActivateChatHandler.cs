using Mediator;
using SharedKernel;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.Exceptions;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;

namespace Tsutskiridze.TradeBuddy.Application.Features.Chats.Commands;

public record ActivateBotCommand(long ChatId, string Token) : ICommand;

public sealed class ActivateChatHandler : ICommandHandler<ActivateBotCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<Chat> _repo;
    private readonly ITelegramSender _sender;

    public ActivateChatHandler(IUnitOfWork uow, IRepository<Chat> repo, ITelegramSender sender)
    {
        _uow = uow;
        _repo = repo;
        _sender = sender;
    }

    public async ValueTask<Unit> Handle(ActivateBotCommand command, CancellationToken cancellationToken)
    {
        var chat = await _repo.FirstOrDefaultAsync(new ChatByActivationTokenSpec(command.Token), cancellationToken)
            ?? throw new ResourceNotFoundException("Invalid activation token");
        
        chat.Activate(command.ChatId);
        await _uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}