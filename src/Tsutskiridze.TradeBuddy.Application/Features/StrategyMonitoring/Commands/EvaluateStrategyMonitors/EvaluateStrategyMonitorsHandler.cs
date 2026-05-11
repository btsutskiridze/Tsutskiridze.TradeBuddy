using Mediator;
using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Specifications;
using Tsutskiridze.TradeBuddy.Application.Notifications;
using Tsutskiridze.TradeBuddy.Domain.Chats;
using Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;

namespace Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Commands.EvaluateDailyStrategyMonitors;

public sealed record EvaluateStrategyMonitorsCommand(DateOnly TradingDate) : ICommand;

public class EvaluateStrategyMonitorsHandler : ICommandHandler<EvaluateStrategyMonitorsCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<StrategyMonitor, int> _strategyMonitors;
    private readonly IRepository<TradeStrategy, int> _strategies;
    private readonly IReadRepository<Chat> _chats;
    private readonly INotificationDispatcher _notifier;
    private readonly IMarketDataProvider _market;
    private readonly IEmaAdxAtrEvaluationCandleBuilder _candleBuilder;

    public EvaluateStrategyMonitorsHandler(
        IUnitOfWork uow,
        IRepository<StrategyMonitor, int> strategyMonitors,
        IRepository<TradeStrategy, int> strategies,
        INotificationDispatcher notifier, IMarketDataProvider market,
        IEmaAdxAtrEvaluationCandleBuilder candleBuilder,
        IReadRepository<Chat> chats)
    {
        _uow = uow;
        _strategyMonitors = strategyMonitors;
        _strategies = strategies;
        _notifier = notifier;
        _market = market;
        _candleBuilder = candleBuilder;
        _chats = chats;
    }

    public async ValueTask<Unit> Handle(
        EvaluateStrategyMonitorsCommand command,
        CancellationToken ct)
    {
        var strategies = await _strategies.ListAsync(new ActiveTradeStrategiesSpec(), ct);
        var strategyMonitors = await _strategyMonitors.ListAsync(
            new ActiveStrategyMonitorsByTradeStrategyIdsSpec(strategies.Select(x => x.Id).ToArray()),
            ct);
        var chatIdsSet =
            (await _chats.ListAsync(new ActiveChatTelegramIdsByIdsSpec(strategyMonitors.Select(x => x.ChatId).ToArray()), ct))
            .ToDictionary(x => x.ChatId, x => x.TelegramId);

        var strategiesDict = strategies.ToDictionary(x => x.Id);

        var notifications = new List<StrategyMonitorAlertNotification>();

        foreach (var strategyMonitor in strategyMonitors)
        {
            var strategy = strategiesDict[strategyMonitor.TradeStrategyId];
            var historyDateRange = await _market.GetClosedDailyDateRange(strategyMonitor.Symbol, ct);
            var historyCandles = await _market.GetDailyCandles(
                strategyMonitor.Symbol,
                historyDateRange.From,
                historyDateRange.To,
                ct
            );

            if (historyCandles[^1].Date == strategyMonitor.LastEvaluatedCandleDate)
                continue;

            var strategyCandles = _candleBuilder.BuildDailyStrategyCandles(
                strategy.EmaTrend.FastPeriod,
                strategy.EmaTrend.SlowPeriod,
                strategy.AdxTrendStrength.Period,
                strategy.AtrStop.Period,
                historyCandles
            );

            var result = EmaAdxAtrStrategyEvaluator.EvaluateLatest(strategy, strategyMonitor, strategyCandles);
            
            strategyMonitor.UpdatePositionState(result.PositionStateAfter);
            strategyMonitor.MarkEvaluated(result.CandleDate);

            notifications.Add(new StrategyMonitorAlertNotification(
                StrategyMonitorEvaluationSummary.Create(
                    chatIdsSet[strategyMonitor.ChatId],
                    strategy.Code.ToString(),
                    strategyMonitor.Symbol,
                    result)
            ));
        }

        await _uow.SaveChangesAsync(ct);
        await _notifier.DispatchAsync(notifications, ct);

        return Unit.Value;
    }
}
