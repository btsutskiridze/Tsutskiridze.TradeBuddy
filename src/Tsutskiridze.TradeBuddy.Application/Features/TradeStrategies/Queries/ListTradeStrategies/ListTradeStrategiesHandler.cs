using Mediator;
using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Services;
using Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Specifications;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.Enums;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.ValueObjects;

namespace Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Queries.ListTradeStrategies;

public sealed record ListTradeStrategiesCommand(long ChatId, string? StrategyCode) : IQuery<ListTradeStrategiesResult>;

public sealed record ListTradeStrategiesResult(IReadOnlyList<TradeStrategyListItem> Strategies);

public sealed record TradeStrategyListItem(
    int Id,
    string Code,
    Timeframe Timeframe,
    int EmaFastPeriod,
    int EmaSlowPeriod,
    int AdxPeriod,
    decimal AdxTrendStrengthThreshold,
    int AdxNonFallingLookBackBars,
    int AtrPeriod,
    decimal AtrInitialStopMultiplier,
    decimal AtrTrailingStopMultiplier,
    decimal AtrTrailingActivationMultiplier);

public sealed class ListTradeStrategiesHandler : IQueryHandler<ListTradeStrategiesCommand, ListTradeStrategiesResult>
{
    private readonly IActiveChatProvider _activeChatProvider;
    private readonly IReadRepository<TradeStrategy, int> _strategies;

    public ListTradeStrategiesHandler(
        IReadRepository<TradeStrategy, int> strategies,
        IActiveChatProvider activeChatProvider)
    {
        _strategies = strategies;
        _activeChatProvider = activeChatProvider;
    }

    public async ValueTask<ListTradeStrategiesResult> Handle(ListTradeStrategiesCommand command, CancellationToken ct)
    {
        var chatId = await _activeChatProvider.GetIdAsync(command.ChatId, ct);
        int? strategyId = command.StrategyCode is null
            ? null
            : TradeStrategyCode.Parse(command.StrategyCode).Id;

        var strategies = await _strategies.ListAsync(
            new ActiveTradeStrategiesByChatIdSpec(chatId, strategyId),
            ct);

        if (strategies.Count == 0)
        {
            return new ListTradeStrategiesResult([]);
        }

        var items = strategies
            .Select(strategy => new TradeStrategyListItem(
                strategy.Id,
                strategy.Code.ToString(),
                strategy.Timeframe,
                strategy.EmaTrend.FastPeriod,
                strategy.EmaTrend.SlowPeriod,
                strategy.AdxTrendStrength.Period,
                strategy.AdxTrendStrength.TrendStrengthThreshold,
                strategy.AdxTrendStrength.NonFallingLookBackBars,
                strategy.AtrStop.Period,
                strategy.AtrStop.InitialStopMultiplier,
                strategy.AtrStop.TrailingStopMultiplier,
                strategy.AtrStop.TrailingActivationMultiplier))
            .ToList();

        return new ListTradeStrategiesResult(items);
    }
}
