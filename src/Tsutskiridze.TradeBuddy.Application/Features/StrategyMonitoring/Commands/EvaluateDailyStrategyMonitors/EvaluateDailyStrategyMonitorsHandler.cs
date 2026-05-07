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

public sealed record EvaluateDailyStrategyMonitorsCommand(DateOnly TradingDate) : ICommand;

public class EvaluateDailyStrategyMonitorsHandler : ICommandHandler<EvaluateDailyStrategyMonitorsCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<StrategyMonitor, int> _monitors;
    private readonly IRepository<TradeStrategy, int> _strategies;
    private readonly IReadRepository<Chat> _chats;
    private readonly INotificationDispatcher _notifier;
    private readonly IMarketDataProvider _market;
    private readonly IEmaAdxAtrEvaluationCandleBuilder _candleBuilder;

    public EvaluateDailyStrategyMonitorsHandler(
        IUnitOfWork uow,
        IRepository<StrategyMonitor, int> monitors,
        IRepository<TradeStrategy, int> strategies,
        INotificationDispatcher notifier, IMarketDataProvider market,
        IEmaAdxAtrEvaluationCandleBuilder candleBuilder,
        IReadRepository<Chat> chats)
    {
        _uow = uow;
        _monitors = monitors;
        _strategies = strategies;
        _notifier = notifier;
        _market = market;
        _candleBuilder = candleBuilder;
        _chats = chats;
    }

    public async ValueTask<Unit> Handle(
        EvaluateDailyStrategyMonitorsCommand command,
        CancellationToken ct)
    {
        var strategies = await _strategies.ListAsync(new ActiveTradeStrategiesSpec(), ct);
        var monitors = await _monitors.ListAsync(
            new ActiveStrategyMonitorsByTradeStrategyIdsSpec(strategies.Select(x => x.Id).ToArray()),
            ct);
        var chatIdsSet =
            (await _chats.ListAsync(new ActiveChatTelegramIdsByIdsSpec(monitors.Select(x => x.ChatId).ToArray()), ct))
            .ToDictionary(x => x.ChatId, x => x.TelegramId);

        var strategiesDict = strategies.ToDictionary(x => x.Id);

        var notifications = new List<EvaluateDailyStrategyMonitorNotification>();

        foreach (var mn in monitors)
        {
            var st = strategiesDict[mn.TradeStrategyId];
            var historyDateRange = await _market.GetClosedDailyDateRange(mn.Symbol, ct);
            var historyCandles = await _market.GetDailyCandles(
                mn.Symbol,
                historyDateRange.From,
                historyDateRange.To,
                ct
            );

            if (historyCandles[^1].Date == mn.LastEvaluatedCandleDate)
                continue;

            var strategyCandles = _candleBuilder.BuildDailyStrategyCandles(
                st.EmaTrend.FastPeriod,
                st.EmaTrend.SlowPeriod,
                st.AdxTrendStrength.Period,
                st.AtrStop.Period,
                historyCandles
            );

            var result = EmaAdxAtrStrategyEvaluator.EvaluateLatest(st, mn, strategyCandles);

            mn.MarkEvaluated(result.CandleDate);

            notifications.Add(new EvaluateDailyStrategyMonitorNotification(
                chatIdsSet[mn.ChatId],
                st.Code.ToString(),
                mn.Symbol,
                result.Action,
                result.PositionSideAfter,
                result.CandleDate,
                result.ClosePrice,
                result.ExecutionPrice ?? mn.PositionState.EntryPrice,
                result.ActiveStop,
                CalculateLongProfitPercent(result, mn),
                result.ShouldNotify,
                result.Reason
            ));
        }

        await _uow.SaveChangesAsync(ct);
        //todo: notification handler
        await _notifier.DispatchAsync(notifications, ct);

        return Unit.Value;
    }

    private static decimal? CalculateLongProfitPercent(StrategyEvaluationResult currentState,
        StrategyMonitor strategyMonitor)
    {
        return currentState.Action is
            StrategyAction.HoldLong or StrategyAction.ExitLongByEmaCross or StrategyAction.ExitLongByStop
            ? (((currentState.ExecutionPrice ?? currentState.ClosePrice) - strategyMonitor.PositionState.EntryPrice) *
               100) / strategyMonitor.PositionState.EntryPrice
            : null;
    }
}