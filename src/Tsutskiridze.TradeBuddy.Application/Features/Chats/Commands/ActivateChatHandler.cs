using Mediator;
using SharedKernel;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.Exceptions;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;

namespace Tsutskiridze.TradeBuddy.Application.Features.Chats.Commands;

public record ActivateBotCommand(long ChatId, string Token) : ICommand<TelegramUpdateResultDto>;

public sealed class ActivateChatHandler : ICommandHandler<ActivateBotCommand, TelegramUpdateResultDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<Chat> _repo;

    public ActivateChatHandler(IUnitOfWork uow, IRepository<Chat> repo)
    {
        _uow = uow;
        _repo = repo;
    }

    public async ValueTask<TelegramUpdateResultDto> Handle(ActivateBotCommand command,
        CancellationToken cancellationToken)
    {
        var chat = await _repo.FirstOrDefaultAsync(new ChatByActivationTokenSpec(command.Token), cancellationToken)
                   ?? throw new ResourceNotFoundException("Invalid activation token");

        chat.Activate(command.ChatId);
        await _uow.SaveChangesAsync(cancellationToken);

        return new TelegramUpdateResultDto()
        {
            ChatId = command.ChatId,
            Text = "StockBuddy Activated Successfully"
        };
    }
}