using System.Data;
using Mediator;
using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Services;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Services;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Specifications;
using Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.Enums;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.ValueObjects;

namespace Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Commands.ActivateStrategyMonitor;

public sealed record ActivateStrategyMonitorCommand(
    long ChatId,
    string StrategyCode,
    string Symbol
) : ICommand<StrategyMonitorEvaluationSummary>;

public class
    ActivateStrategyMonitorHandler : ICommandHandler<ActivateStrategyMonitorCommand, StrategyMonitorEvaluationSummary>
{
    private readonly IUnitOfWork _uow;
    private readonly IActiveChatProvider _activeChatProvider;
    private readonly IReadRepository<TradeStrategy, int> _strategies;
    private readonly IRepository<StrategyMonitor, int> _monitors;
    private readonly IStockService _stocks;
    private readonly IMarketDataProvider _market;
    private readonly IEmaAdxAtrEvaluationCandleBuilder _emaAdxAtrEvaluationCandleBuilder;

    public ActivateStrategyMonitorHandler(
        IUnitOfWork uow,
        IActiveChatProvider activeChatProvider,
        IReadRepository<TradeStrategy, int> strategies,
        IRepository<StrategyMonitor, int> monitors,
        IStockService stocks,
        IMarketDataProvider market,
        IEmaAdxAtrEvaluationCandleBuilder emaAdxAtrEvaluationCandleBuilder)
    {
        _uow = uow;
        _activeChatProvider = activeChatProvider;
        _strategies = strategies;
        _monitors = monitors;
        _stocks = stocks;
        _market = market;
        _emaAdxAtrEvaluationCandleBuilder = emaAdxAtrEvaluationCandleBuilder;
    }


    public async ValueTask<StrategyMonitorEvaluationSummary> Handle(ActivateStrategyMonitorCommand cmd,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var chatId = await _activeChatProvider.GetIdAsync(cmd.ChatId, ct);
        var (stockQuote, historyDateRange) = await FetchStockQuoteDetails(cmd.Symbol, ct);

        await using var tx = await _uow.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        var stock = await _stocks.GetOrCreateStock(cmd.Symbol, stockQuote.Name, stockQuote.Currency, ct);
        var strategy = await GetValidatedStrategyId(cmd.StrategyCode, chatId, ct);

        var strategyMonitor =
            await _monitors.FirstOrDefaultAsync(
                new StrategyMonitorByChatStrategyStockSpec(chatId, strategy.Id, stock.Id), ct);

        if (strategyMonitor is null)
        {
            strategyMonitor = new StrategyMonitor(
                chatId,
                strategy.Id,
                stock.Id,
                stock.Symbol,
                Timeframe.Daily,
                now
            );

            await _monitors.AddAsync(strategyMonitor, ct);
        }
        else
        {
            strategyMonitor.Activate();
        }

        var historyCandles = await _market.GetDailyCandles(
            cmd.Symbol,
            historyDateRange.From,
            historyDateRange.To,
            ct
        );
        var strategyCandles = _emaAdxAtrEvaluationCandleBuilder.BuildDailyStrategyCandles(
            strategy.EmaTrend.FastPeriod,
            strategy.EmaTrend.SlowPeriod,
            strategy.AdxTrendStrength.Period,
            strategy.AtrStop.Period,
            historyCandles
        );

        var results = EmaAdxAtrStrategyEvaluator.Replay(strategy, strategyMonitor, strategyCandles);
        var currentState = results[^1];

        strategyMonitor.UpdatePositionState(currentState.PositionStateAfter);
        strategyMonitor.MarkEvaluated(currentState.CandleDate);

        await _uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return StrategyMonitorEvaluationSummary.Create(
            cmd.ChatId,
            strategy.Code.ToString(),
            stock.Symbol,
            currentState);
    }

    private async Task<(StockQuote stockQuote, MarketHistoryDateRange historyDateRange)> FetchStockQuoteDetails(
        string symbol,
        CancellationToken ct)
    {
        var stockQuote = await _market.GetStockQuote(symbol, ct)
                         ?? throw new ResourceNotFoundException("Stock symbol not found");
        var historyDateRange = await _market.GetClosedDailyDateRange(symbol, ct);

        return (stockQuote, historyDateRange);
    }

    private async Task<TradeStrategy> GetValidatedStrategyId(string strategyCode, Guid chatId, CancellationToken ct)
    {
        var tradeStrategyCode = TradeStrategyCode.Parse(strategyCode);
        var strategy = await _strategies.FirstOrDefaultAsync(
            new ActiveTradeStrategyByChatIdAndIdSpec(chatId, tradeStrategyCode.Id),
            ct
        ) ?? throw new ResourceNotFoundException("Trade strategy not found.");

        return strategy;
    }
}