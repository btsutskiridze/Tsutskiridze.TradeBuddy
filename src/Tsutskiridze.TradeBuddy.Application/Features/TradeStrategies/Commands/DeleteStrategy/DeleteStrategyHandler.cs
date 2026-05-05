using Mediator;
using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Services;
using Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Specifications;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.ValueObjects;

namespace Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Commands.DeleteStrategy;

public sealed record DeleteStrategyCommand(long ChatId, string StrategyCode) : ICommand<DeleteStrategyResult>;

public sealed record DeleteStrategyResult(string StrategyCode);

public sealed class DeleteStrategyHandler : ICommandHandler<DeleteStrategyCommand, DeleteStrategyResult>
{
    private readonly IActiveChatProvider _activeChatProvider;
    private readonly IRepository<TradeStrategy, int> _strategies;
    private readonly IUnitOfWork _uow;

    public DeleteStrategyHandler(
        IRepository<TradeStrategy, int> strategies,
        IActiveChatProvider activeChatProvider,
        IUnitOfWork uow)
    {
        _strategies = strategies;
        _activeChatProvider = activeChatProvider;
        _uow = uow;
    }

    public async ValueTask<DeleteStrategyResult> Handle(DeleteStrategyCommand command, CancellationToken ct)
    {
        var chatId = await _activeChatProvider.GetIdAsync(command.ChatId, ct);
        var strategyCode = TradeStrategyCode.Parse(command.StrategyCode);
        var strategy = await _strategies.FirstOrDefaultAsync(
            new ActiveTradeStrategyByChatIdAndIdSpec(chatId, strategyCode.Id),
            ct);

        if (strategy is null)
        {
            throw new ResourceNotFoundException("Trade strategy not found.");
        }

        _strategies.Remove(strategy);
        
        await _uow.SaveChangesAsync(ct);

        return new DeleteStrategyResult(strategyCode.ToString());
    }
}
