using Mediator;
using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Specifications;
using Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;

namespace Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Commands.EvaluateStrategyMonitors;

public sealed record EvaluateStrategyMonitorsCommand(DateOnly TradingDate) : ICommand;

public class EvaluateStrategyMonitorsCommandHandler : ICommandHandler<EvaluateStrategyMonitorsCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<StrategyMonitor, int> _strategyMonitors;
    private readonly IRepository<TradeStrategy, int> _strategies;
    private readonly IMarketDataProvider _market;
    private readonly IEmaAdxAtrEvaluationCandleBuilder _candleBuilder;

    public EvaluateStrategyMonitorsCommandHandler(
        IUnitOfWork uow,
        IRepository<StrategyMonitor, int> strategyMonitors,
        IRepository<TradeStrategy, int> strategies,
        IMarketDataProvider market,
        IEmaAdxAtrEvaluationCandleBuilder candleBuilder)
    {
        _uow = uow;
        _strategyMonitors = strategyMonitors;
        _strategies = strategies;
        _market = market;
        _candleBuilder = candleBuilder;
    }

    public async ValueTask<Unit> Handle(
        EvaluateStrategyMonitorsCommand command,
        CancellationToken ct)
    {
        var strategies = await _strategies.ListAsync(new ActiveTradeStrategiesSpec(), ct);
        var strategyMonitors = await _strategyMonitors.ListAsync(
            new ActiveStrategyMonitorsByTradeStrategyIdsSpec(strategies.Select(x => x.Id).ToArray()),
            ct);

        var strategiesDict = strategies.ToDictionary(x => x.Id);

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

            strategyMonitor.ApplyEvaluation(result);
        }

        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
