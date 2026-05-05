using Mediator;
using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Services;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.Enums;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.ValueObjects;

namespace Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Commands.CreateStrategy;

public sealed record CreateStrategyCommand(
    long ChatId,
    Timeframe Timeframe,
    int EmaFastPeriod,
    int EmaSlowPeriod,
    int AdxPeriod,
    decimal AdxTrendStrengthThreshold,
    int AdxNonFallingLookBackBars,
    int AtrPeriod,
    decimal AtrInitialStopMultiplier,
    decimal AtrTrailingStopMultiplier,
    int AtrTrailingActivationMultiplier) : ICommand<CreateStrategyResult>;

public sealed record CreateStrategyResult(string StrategyId);

public sealed class CreateStrategyHandler : ICommandHandler<CreateStrategyCommand, CreateStrategyResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<TradeStrategy, int> _strategies;
    private readonly IActiveChatProvider _chat;

    public CreateStrategyHandler(IRepository<TradeStrategy, int> strategies, IActiveChatProvider chat, IUnitOfWork uow)
    {
        _strategies = strategies;
        _chat = chat;
        _uow = uow;
    }

    public async ValueTask<CreateStrategyResult> Handle(CreateStrategyCommand cmd, CancellationToken ct)
    {
        var chatId = await _chat.GetIdAsync(cmd.ChatId, ct);

        var strategy = new TradeStrategy(
            chatId,
            cmd.Timeframe,
            new EmaTrendSettings(cmd.EmaFastPeriod, cmd.EmaSlowPeriod),
            new AdxTrendStrengthSettings(cmd.AdxPeriod, cmd.AdxTrendStrengthThreshold, cmd.AdxNonFallingLookBackBars),
            new AtrStopSettings(cmd.AtrPeriod,cmd.AtrInitialStopMultiplier, cmd.AtrTrailingStopMultiplier, cmd.AtrTrailingActivationMultiplier)
        );

        await _strategies.AddAsync(strategy, ct);

        await _uow.SaveChangesAsync(ct);
        
        return new CreateStrategyResult(strategy.Code);
    }
}